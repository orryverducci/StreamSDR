/*
 * This file is part of StreamSDR.
 *
 * StreamSDR is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * StreamSDR is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with StreamSDR. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StreamSDR.Radios.RtlSdr;

/// <summary>
/// Provides access to control and receive samples from a rtl-sdr radio.
/// </summary>
internal sealed class Radio : RadioBase
{
    #region Private fields
    /// <summary>
    /// The device handle.
    /// </summary>
    private IntPtr _device;

    /// <summary>
    /// The worker thread used to receive samples from the radio.
    /// </summary>
    private readonly Thread _receiverThread;

    /// <summary>
    /// The callback used to process the samples that have been read.
    /// </summary>
    private readonly Interop.ReadDelegate _readCallback;

    /// <summary>
    /// An array of the gain values in dB supported by the tuner.
    /// </summary>
    private int[] _gains = Array.Empty<int>();

    /// <summary>
    /// The mode in which the radio's gain is operating.
    /// </summary>
    private GainMode _gainMode = GainMode.Automatic;

    /// <summary>
    /// If the digital AGC of the RTL2832 is enabled.
    /// </summary>
    private bool _rtlAgc = false;

    /// <summary>
    /// If the bias tee has been enabled.
    /// </summary>
    private bool _biasTee = false;
    #endregion

    #region Constructor, finaliser and lifecycle methods
    /// <summary>
    /// Initialises a new instance of the <see cref="Radio"/> class.
    /// </summary>
    /// <param name="logger">The logger for the <see cref="Radio"/> class.</param>
    /// <param name="lifetime">The application lifetime service.</param>
    /// <param name="config">The application configuration.</param>
    public unsafe Radio(ILogger<Radio> logger, IHostApplicationLifetime lifetime, IConfiguration config) : base(logger, lifetime, config)
    {
        // Create the sample receiver worker thread
        _receiverThread = new(ReceiverWorker)
        {
            Name = "ReceiverThread"
        };

        // Set the sample reading callback
        _readCallback = new Interop.ReadDelegate(ProcessSamples);
    }

    /// <inheritdoc/>
    public override void Start()
    {
        // Log that the radio is starting
        _logger.LogInformation("Starting the rtl-sdr radio");

        try
        {
            // Check if a rtl-sdr device is available
            if (Interop.GetDeviceCount() < 1)
            {
                _logger.LogCritical("No rtl-sdr devices could be found");
                _applicationLifetime.StopApplication();
                return;
            }

            // Find the ID of the specified device, or use the first one if no device is specified
            int deviceId = 0;
            string serial = _config.GetValue<string>("serial");

            if (serial != null)
            {
                deviceId = Interop.GetDeviceIndexBySerial(serial);

                if (deviceId < 0)
                {
                    _logger.LogCritical($"Could not find a rtl-sdr device with the serial {serial}");
                    _applicationLifetime.StopApplication();
                    return;
                }
            }

            // Get the device name
            Name = Interop.GetDeviceName((byte)deviceId);

            // Open the device
            if (Interop.Open(out _device, (byte)deviceId) != 0)
            {
                _logger.LogCritical("The rtl-sdr device could not be opened");
                _applicationLifetime.StopApplication();
                return;
            }

            // Get the tuner type
            Tuner = Interop.GetTunerType(_device) switch
            {
                RtlSdr.Tuner.E4000 => TunerType.E4000,
                RtlSdr.Tuner.FC0012 => TunerType.FC0012,
                RtlSdr.Tuner.FC0013 => TunerType.FC0013,
                RtlSdr.Tuner.FC2580 => TunerType.FC2580,
                RtlSdr.Tuner.R820T => TunerType.R820T,
                RtlSdr.Tuner.R828D => TunerType.R828D,
                _ => TunerType.Unknown
            };

            // Get the gain values supported by the tuner
            int numberOfGains = Interop.GetTunerGains(_device, null);
            int[] gains = new int[numberOfGains];
            if (Interop.GetTunerGains(_device, gains) != 0)
            {
                _gains = gains;
                GainLevelsSupported = (uint)_gains.Length;
            }
            else
            {
                _logger.LogError("Unable to get the gain values supported by the tuner");
            }

            // Set the initial state
            BiasTee = false;
            Frequency = DefaultFrequency;
            SampleRate = DefaultSampleRate;
            DirectSampling = DirectSamplingMode.Off;
            Gain = 0;
            AutomaticGainCorrection = false;
            GainMode = GainMode.Automatic;

            // Start the receiver thread
            _receiverThread.Start();

            // Log that the radio has started
            _logger.LogInformation($"Started the radio: {Name}");
        }
        catch (DllNotFoundException)
        {
            _logger.LogCritical("Unable to find the rtlsdr library");
            _applicationLifetime.StopApplication();
        }
        catch (BadImageFormatException)
        {
            _logger.LogCritical("The rtlsdr library or one of its dependencies has been built for the wrong system architecture");
            _applicationLifetime.StopApplication();
        }
    }

    /// <inheritdoc/>
    public override void Stop()
    {
        // Check that the device has been started
        if (_device == IntPtr.Zero)
        {
            return;
        }

        // Log that the radio is stopping
        _logger.LogInformation($"Stopping the rtl-sdr radio ({Name})");

        // Stop reading samples from the device
        Interop.CancelAsync(_device);
        _receiverThread.Join();

        // Close the device
        Interop.Close(_device);

        // Clear the device handle and name
        _device = IntPtr.Zero;
        Name = string.Empty;

        // Log that the radio has stopped
        _logger.LogInformation($"The radio has stopped");
    }
    #endregion

    #region Radio parameter methods
    /// <inheritdoc/>
    protected override uint GetSampleRate() => _device != IntPtr.Zero ? Interop.GetSampleRate(_device) : 0;

    /// <inheritdoc/>
    protected override int SetSampleRate(uint sampleRate)
    {
        if (_device != IntPtr.Zero)
        {
            return Interop.SetSampleRate(_device, sampleRate);
        }
        else
        {
            return int.MinValue;
        }
    }

    /// <inheritdoc/>
    protected override ulong GetFrequency() => _device != IntPtr.Zero ? Interop.GetCenterFreq(_device) : 0;

    /// <inheritdoc/>
    protected override int SetFrequency(ulong frequency)
    {
        if (_device != IntPtr.Zero)
        {
            return Interop.SetCenterFreq(_device, (uint)frequency);
        }
        else
        {
            return int.MinValue;
        }
    }

    /// <inheritdoc/>
    protected override int GetFrequencyCorrection() => _device != IntPtr.Zero ? Interop.GetFreqCorrection(_device) : 0;

    /// <inheritdoc/>
    protected override int SetFrequencyCorrection(int freqCorrection)
    {
        if (_device == IntPtr.Zero)
        {
            return int.MinValue;
        }

        int result = Interop.SetFreqCorrection(_device, freqCorrection);

        // We ignore error -2, which indicates the correction is already set to given value, so we return 0 instead
        if (result == -2)
        {
            _logger.LogDebug($"Received error -2 when setting the frequency correction, indicating it is already set to {freqCorrection.ToString("N0", Thread.CurrentThread.CurrentCulture)} ppm, so it is being ignored");
            return 0;
        }

        return result;
    }

    /// <inheritdoc/>
    protected override bool GetOffsetTuning()
    {
        if (_device != IntPtr.Zero)
        {
            return Interop.GetOffsetTuning(_device) == 1 ? true : false;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    protected override int SetOffsetTuning(bool enabled)
    {
        if (_device == IntPtr.Zero)
        {
            return int.MinValue;
        }

        if (Tuner == TunerType.R820T || Tuner == TunerType.R828D)
        {
            return base.SetOffsetTuning(enabled);
        }

        int offsetTuning = enabled ? 1 : 0;
        return Interop.SetOffsetTuning(_device, offsetTuning);
    }

    /// <inheritdoc/>
    protected override DirectSamplingMode GetDirectSampling()
    {
        if (_device != IntPtr.Zero)
        {
            return Interop.GetDirectSampling(_device) switch
            {
                1 => DirectSamplingMode.IBranch,
                2 => DirectSamplingMode.QBranch,
                _ => DirectSamplingMode.Off,
            };
        }
        else
        {
            return DirectSamplingMode.Off;
        }
    }

    /// <inheritdoc/>
    protected override int SetDirectSampling(DirectSamplingMode mode)
    {
        if (_device != IntPtr.Zero)
        {
            int directSampling = mode switch
            {
                DirectSamplingMode.IBranch => 1,
                DirectSamplingMode.QBranch => 2,
                _ => 0,
            };

            return Interop.SetDirectSampling(_device, directSampling);
        }
        else
        {
            return int.MinValue;
        }
    }

    /// <inheritdoc/>
    protected override uint GetGain()
    {
        if (_device == IntPtr.Zero)
        {
            return 0;
        }

        int i = Array.IndexOf(_gains, Interop.GetTunerGain(_device));

        return i >= 0 ? (uint)i : 0;
    }

    /// <inheritdoc/>
    protected override int SetGain(uint level)
    {
        if (_device != IntPtr.Zero)
        {
            return Interop.SetTunerGain(_device, _gains[level]);
        }
        else
        {
            return int.MinValue;
        }
    }

    /// <inheritdoc/>
    protected override GainMode GetGainMode() => _gainMode;

    /// <inheritdoc/>
    protected override int SetGainMode(GainMode mode)
    {
        if (_device == IntPtr.Zero)
        {
            return int.MinValue;
        }

        int gainMode = mode == GainMode.Manual ? 1 : 0;
        int result = Interop.SetTunerGainMode(_device, gainMode);

        if (result == 0)
        {
            _gainMode = mode;
        }

        return result;
    }

    /// <inheritdoc/>
    protected override bool GetAgc() => _rtlAgc;

    /// <inheritdoc/>
    protected override int SetAgc(bool enabled)
    {
        if (_device == IntPtr.Zero)
        {
            return int.MinValue;
        }

        int rtlAgc = enabled ? 1 : 0;
        int result = Interop.SetAGCMode(_device, rtlAgc);

        if (result == 0)
        {
            _rtlAgc = enabled;
        }

        return result;
    }

    /// <inheritdoc/>
    protected override bool GetBiasTee() => _biasTee;

    /// <inheritdoc/>
    protected override int SetBiasTee(bool enabled)
    {
        if (_device == IntPtr.Zero)
        {
            return int.MinValue;
        }

        int biasTee = enabled ? 1 : 0;
        int result = Interop.SetBiasTee(_device, biasTee);

        if (result == 0)
        {
            _biasTee = enabled;
        }

        return result;
    }
    #endregion

    #region Sample handling methods
    /// <summary>
    /// Worker for the receiver thead. Starts the read functionality provided by the rtl-sdr library.
    /// </summary>
    private void ReceiverWorker()
    {
        // Reset the device sample buffer
        Interop.ResetBuffer(_device);

        // Start reading samples
        Interop.ReadAsync(_device, _readCallback, IntPtr.Zero, 0, 0);
    }

    /// <summary>
    /// The callback method called by the rtl-sdr library to provide received samples.
    /// </summary>
    /// <param name="buf">The buffer of samples.</param>
    /// <param name="len">The length of the buffer.</param>
    /// <param name="ctx">The user context passed to the read function.</param>
    private unsafe void ProcessSamples(byte* buf, uint len, IntPtr ctx)
    {
        // Wrap the buffer in a span
        Span<byte> buffer = new(buf, (int)len);

        // Copy the buffer to a new array of bytes in managed memory
        byte[] bufferArray = buffer.ToArray();

        // Send the samples to the clients
        SendSamplesToClients(bufferArray);
    }
    #endregion
}
