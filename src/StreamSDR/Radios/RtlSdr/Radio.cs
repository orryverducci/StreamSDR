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

using System;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StreamSDR.Radios.RtlSdr
{
    /// <summary>
    /// Provides access to control and receive samples from a rtl-sdr radio.
    /// </summary>
    internal class Radio : IRadio
    {
        #region Constants
        /// <summary>
        /// The default frequency (100 MHz)
        /// </summary>
        private const uint DefaultFrequency = 100000000;

        /// <summary>
        /// The default sample rate (2.048 MHz)
        /// </summary>
        private const uint DefaultSampleRate = 2048000;
        #endregion

        #region Private fields
        /// <summary>
        /// <see langword="true"/> if Dispose() has been called, <see langword="false"/> otherwise.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The application lifetime service.
        /// </summary>
        private readonly IHostApplicationLifetime _applicationLifetime;

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

        #region Properties
        /// <inheritdoc/>
        public string Name { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public uint SampleRate
        {
            get => _device != IntPtr.Zero ? Interop.GetSampleRate(_device) : 0;
            set
            {
                _logger.LogInformation($"Setting the sample rate to {value.ToString("N0", Thread.CurrentThread.CurrentCulture)} Hz");

                if (_device == IntPtr.Zero || Interop.SetSampleRate(_device, value) != 0)
                {
                    _logger.LogError("Unable to set the sample rate");
                }
            }
        }

        /// <inheritdoc/>
        public TunerType Tuner
        {
            get
            {
                if (_device != IntPtr.Zero)
                {
                    return Interop.GetTunerType(_device) switch
                    {
                        RtlSdr.Tuner.E4000 => TunerType.E4000,
                        RtlSdr.Tuner.FC0012 => TunerType.FC0012,
                        RtlSdr.Tuner.FC0013 => TunerType.FC0013,
                        RtlSdr.Tuner.FC2580 => TunerType.FC2580,
                        RtlSdr.Tuner.R820T => TunerType.R820T,
                        RtlSdr.Tuner.R828D => TunerType.R828D,
                        _ => TunerType.Unknown
                    };
                }
                else
                {
                    return TunerType.Unknown;
                }
            }
        }

        /// <inheritdoc/>
        public ulong Frequency
        {
            get => _device != IntPtr.Zero ? Interop.GetCenterFreq(_device) : 0;
            set
            {
                NumberFormatInfo numberFormat = new NumberFormatInfo
                {
                    NumberGroupSeparator = "."
                };
                _logger.LogInformation($"Setting the frequency to {value.ToString("N0", numberFormat)} Hz");

                if (_device == IntPtr.Zero || Interop.SetCenterFreq(_device, (uint)value) != 0)
                {
                    _logger.LogError("Unable to set the centre frequency");
                }
            }
        }

        /// <inheritdoc/>
        public int FrequencyCorrection
        {
            get => _device != IntPtr.Zero ? Interop.GetFreqCorrection(_device) : 0;
            set
            {
                _logger.LogInformation($"Setting the frequency correction to {value.ToString("N0", Thread.CurrentThread.CurrentCulture)} ppm");

                if (_device == IntPtr.Zero || Interop.SetFreqCorrection(_device, value) != 0)
                {
                    _logger.LogError("Unable to set the frequency correction");
                }
            }
        }

        /// <inheritdoc/>
        public bool OffsetTuning
        {
            get
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
            set
            {
                int offsetTuning = value ? 1 : 0;
                string state = value ? "on" : "off";

                _logger.LogInformation($"Turning {state} offset tuning");

                if (_device != IntPtr.Zero)
                {
                    TunerType tunerType = Tuner;
                    if (tunerType == TunerType.R820T || tunerType == TunerType.R828D)
                    {
                        _logger.LogError($"Unable to set offset tuning as it is not supported by this type of tuner");
                        return;
                    }

                    if (Interop.SetOffsetTuning(_device, offsetTuning) == 0)
                    {
                        return;
                    }
                }

                // If not returned yet, there is an error
                _logger.LogError("Unable to set offset tuning");
            }
        }

        /// <inheritdoc/>
        public DirectSamplingMode DirectSampling
        {
            get
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
            set
            {
                _logger.LogInformation($"Setting direct sampling to {value}");

                int directSampling = value switch
                {
                    DirectSamplingMode.IBranch => 1,
                    DirectSamplingMode.QBranch => 2,
                    _ => 0,
                };

                if (_device == IntPtr.Zero || Interop.SetDirectSampling(_device, directSampling) != 0)
                {
                    _logger.LogError("Unable to set the direct sampling mode");
                }
            }
        }

        /// <inheritdoc/>
        public uint Gain
        {
            get
            {
                if (_device != IntPtr.Zero)
                {
                    return 0;
                }

                int i = Array.IndexOf<int>(_gains, Interop.GetTunerGain(_device));

                return i >= 0 ? (uint)i : 0;
            }
            set
            {
                _logger.LogInformation($"Setting the gain to level {value}");

                if (_device == IntPtr.Zero || value > _gains.Length || Interop.SetTunerGain(_device, _gains[value]) != 0)
                {
                    _logger.LogError("Unable to set the gain");
                }
            }
        }

        /// <inheritdoc/>
        public GainMode GainMode
        {
            get => _gainMode;
            set
            {
                _logger.LogInformation($"Setting the gain mode to {value}");

                int gainMode = value == GainMode.Manual ? 1 : 0;

                if (_device == IntPtr.Zero || Interop.SetTunerGainMode(_device, gainMode) != 0)
                {
                    _logger.LogError("Unable to set the gain mode");
                    return;
                }

                _gainMode = value;
            }
        }

        /// <inheritdoc/>
        public uint GainLevelsSupported => (uint)_gains.Length;

        /// <inheritdoc/>
        public bool AutomaticGainCorrection
        {
            get => _rtlAgc;
            set
            {
                int rtlAgc = value ? 1 : 0;
                string state = value ? "on" : "off";

                _logger.LogInformation($"Setting the RTL AGC to {state}");

                if (_device == IntPtr.Zero || Interop.SetAGCMode(_device, rtlAgc) != 0)
                {
                    _logger.LogError("Unable to set the RTL AGC");
                    return;
                }

                _rtlAgc = value;
            }
        }

        /// <inheritdoc/>
        public bool BiasTee
        {
            get => _biasTee;
            set
            {
                int biasTee = value ? 1 : 0;
                string state = value ? "on" : "off";

                _logger.LogInformation($"Turning {state} the bias tee");

                if (_device == IntPtr.Zero || Interop.SetBiasTee(_device, biasTee) != 0)
                {
                    _logger.LogError("Unable to set bias tee");
                }

                _biasTee = value;
            }
        }
        #endregion

        #region Events
        /// <inheritdoc/>
        public event EventHandler<byte[]>? SamplesAvailable;
        #endregion

        #region Constructor, finaliser and lifecycle methods
        /// <summary>
        /// Initialises a new instance of the <see cref="Radio"/> class.
        /// </summary>
        public unsafe Radio(ILogger<Radio> logger, IHostApplicationLifetime lifetime)
        {
            // Store a reference to the logger
            _logger = logger;

            // Store a reference to the application lifetime
            _applicationLifetime = lifetime;

            // Create the sample receiver worker thread
            _receiverThread = new(ReceiverWorker)
            {
                Name = "ReceiverThread"
            };

            // Set the sample reading callback
            _readCallback = new Interop.ReadDelegate(ProcessSamples);
        }

        /// <summary>
        /// Finalises the instance of the <see cref="RtlSdrRadio"/> class.
        /// </summary>
        ~Radio() => Dispose();

        /// <inheritdoc/>
        public void Dispose()
        {
            // Return if already disposed
            if (_disposed)
            {
                return;
            }

            // Stop the device if it is running
            Stop();

            // Set that dispose has run
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void Start()
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

                // Get the device name
                Name = Interop.GetDeviceName(0);

                // Open the device
                if (Interop.Open(out _device, 0) != 0)
                {
                    _logger.LogCritical("The rtl-sdr device could not be opened");
                    _applicationLifetime.StopApplication();
                    return;
                }

                // Get the gain values supported by the tuner
                int numberOfGains = Interop.GetTunerGains(_device, null);
                int[] gains = new int[numberOfGains];
                if (Interop.GetTunerGains(_device, gains) == 0)
                {
                    _gains = gains;
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
        public void Stop()
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

            // Fire the samples available event
            SamplesAvailable?.Invoke(this, bufferArray);
        }
        #endregion
    }
}
