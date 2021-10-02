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
                if (_device != IntPtr.Zero && Interop.SetSampleRate(_device, value) == 0)
                {
                    _logger.LogInformation($"Setting the sample rate to {value.ToString("N0", Thread.CurrentThread.CurrentCulture)} Hz");
                }
                else
                {
                    _logger.LogError($"Unable to set the sample rate to {value.ToString("N0", Thread.CurrentThread.CurrentCulture)} Hz");
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
                    switch (Interop.GetTunerType(_device))
                    {
                        case RtlSdr.Tuner.E4000:
                            return TunerType.E4000;
                        case RtlSdr.Tuner.FC0012:
                            return TunerType.FC0012;
                        case RtlSdr.Tuner.FC0013:
                            return TunerType.FC0013;
                        case RtlSdr.Tuner.FC2580:
                            return TunerType.FC2580;
                        case RtlSdr.Tuner.R820T:
                            return TunerType.R820T;
                        case RtlSdr.Tuner.R828D:
                            return TunerType.R828D;
                        default:
                            return TunerType.Unknown;
                    }
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
                NumberFormatInfo numberFormat = new NumberFormatInfo();
                numberFormat.NumberGroupSeparator = ".";

                if (_device != IntPtr.Zero && Interop.SetCenterFreq(_device, (uint)value) == 0)
                {
                    _logger.LogInformation($"Setting the frequency to {value.ToString("N0", numberFormat)} Hz");
                }
                else
                {
                    _logger.LogError($"Unable to set the centre frequency to {value.ToString("N0", numberFormat)} Hz");
                }
            }
        }

        /// <inheritdoc/>
        public int FrequencyCorrection
        {
            get => _device != IntPtr.Zero ? Interop.GetFreqCorrection(_device) : 0;
            set
            {
                if (_device != IntPtr.Zero && Interop.SetFreqCorrection(_device, value) == 0)
                {
                    _logger.LogInformation($"Setting the frequency correction to {value.ToString("N0", Thread.CurrentThread.CurrentCulture)} ppm");
                }
                else
                {
                    _logger.LogError($"Unable to set the frequency correction to {value.ToString("N0", Thread.CurrentThread.CurrentCulture)} ppm");
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
                        _logger.LogInformation($"Turning {state} offset tuning");
                    }
                    else
                    {
                        _logger.LogError($"Unable to turn {state} offset tuning");
                    }
                }
            }
        }

        /// <inheritdoc/>
        public DirectSamplingMode DirectSampling
        {
            get
            {
                if (_device != IntPtr.Zero)
                {
                    DirectSamplingMode mode;

                    switch (Interop.GetDirectSampling(_device))
                    {
                        case 1:
                            mode = DirectSamplingMode.IBranch;
                            break;
                        case 2:
                            mode = DirectSamplingMode.QBranch;
                            break;
                        default:
                            mode = DirectSamplingMode.Off;
                            break;
                    }

                    return mode;
                }
                else
                {
                    return DirectSamplingMode.Off;
                }
            }
            set
            {
                int directSampling;

                switch (value)
                {
                    case DirectSamplingMode.IBranch:
                        directSampling = 1;
                        break;
                    case DirectSamplingMode.QBranch:
                        directSampling = 2;
                        break;
                    default:
                        directSampling = 0;
                        break;
                }

                if (_device != IntPtr.Zero && Interop.SetDirectSampling(_device, directSampling) == 0)
                {
                    _logger.LogInformation($"Setting direct sampling to {value}");
                }
                else
                {
                    _logger.LogError($"Unable to set direct sampling to {value}");
                }
            }
        }

        /// <inheritdoc/>
        public float Gain
        {
            get => _device != IntPtr.Zero ? Interop.GetTunerGain(_device) / 10f : 0f;
            set
            {
                int gain = (int)MathF.Floor(value * 10);

                if (_device != IntPtr.Zero && Interop.SetTunerGain(_device, gain) == 0)
                {
                    _logger.LogInformation($"Setting the gain to {value} dB");
                }
                else
                {
                    _logger.LogError($"Unable to set the gain to {value} dB");
                }
            }
        }

        /// <inheritdoc/>
        public GainMode GainMode
        {
            get => _gainMode;
            set
            {
                int gainMode = value == GainMode.Manual ? 1 : 0;

                if (_device != IntPtr.Zero && Interop.SetTunerGainMode(_device, gainMode) == 0)
                {
                    _gainMode = value;
                    _logger.LogInformation($"Setting the gain mode to {value}");
                }
                else
                {
                    _logger.LogError($"Unable to set the gain mode to {value}");
                }
            }
        }

        /// <inheritdoc/>
        public float[] GainLevelsSupported
        {
            get
            {
                if (_device == IntPtr.Zero)
                {
                    _logger.LogError($"Unable to get the levels of gain supported by the tuner");
                    return Array.Empty<float>();
                }

                // Get the number of gains supported by the tuner
                int numberOfGains = Interop.GetTunerGains(_device, null);

                // If the number of gains is 0 or negative, return an error
                if (numberOfGains < 1)
                {
                    _logger.LogError($"Unable to get the levels of gain supported by the tuner");
                    return Array.Empty<float>();
                }

                // Get the supported gains
                int[] gains = new int[numberOfGains];
                Interop.GetTunerGains(_device, gains);

                // Convert to floats and return
                return Array.ConvertAll(gains, item => item / 10f);
            }
        }

        /// <inheritdoc/>
        public bool AutomaticGainCorrection
        {
            get => _rtlAgc;
            set
            {
                int rtlAgc = value ? 1 : 0;
                string state = value ? "on" : "off";

                if (_device != IntPtr.Zero && Interop.SetAGCMode(_device, rtlAgc) == 0)
                {
                    _rtlAgc = value;
                    
                    _logger.LogInformation($"Setting the RTL AGC to {state}");
                }
                else
                {
                    _logger.LogError($"Unable to set the RTL AGC to {state}");
                }
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

                if (_device != IntPtr.Zero && Interop.SetBiasTee(_device, biasTee) == 0)
                {
                    _biasTee = value;

                    _logger.LogInformation($"Turning {state} the bias tee");
                }
                else
                {
                    _logger.LogError($"Unable to turn {state} the bias tee");
                }
            }
        }
        #endregion

        #region Events
        /// <inheritdoc/>
        public event EventHandler<byte[]>? SamplesAvailable;
        #endregion

        #region Constructor, finaliser and lifecycle methods
        /// <summary>
        /// Initialises a new instance of the <see cref="RtlSdrRadio"/> class.
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

                // Set the initial state
                BiasTee = false;
                Frequency = DefaultFrequency;
                SampleRate = DefaultSampleRate;
                DirectSampling = DirectSamplingMode.Off;
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
