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

using System.Globalization;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StreamSDR.Radios.SdrPlay.Callbacks;
using StreamSDR.Radios.SdrPlay.Hardware;

namespace StreamSDR.Radios.SdrPlay;

/// <summary>
/// Provides access to control and receive samples from a SDRplay radio.
/// </summary>
internal unsafe class Radio : IRadio
{
    #region Constants
    /// <summary>
    /// The SDRplay API version this has been developed for.
    /// </summary>
    private const float SdrPlayApiVersion = 3.07f;

    /// <summary>
    /// The default frequency (100 MHz)
    /// </summary>
    private const uint DefaultFrequency = 100000000;

    /// <summary>
    /// The default sample rate (2.048 MHz)
    /// </summary>
    private const uint DefaultSampleRate = 2048000;

    /// <summary>
    /// The maximum decimation factor that can be used for sample rates smaller than 2 MHz.
    /// </summary>
    private const uint MaxDecimationFactor = 64;
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
    /// Indicates if the SDRplay API has been locked by the application.
    /// </summary>
    private bool _apiLocked = false;

    /// <summary>
    /// Bit depth converter to convert samples to 8 bit.
    /// </summary>
    private DSP.ShortToByteBitDepthConversion? _bitConverter;

    /// <summary>
    /// The selected device, containing the device handle.
    /// </summary>
    private Device _device;

    /// <summary>
    /// The pointer to the device parameters.
    /// </summary>
    private Parameters.DeviceParams* _deviceParams;

    /// <summary>
    /// <see langword="true"/> if the device has been initialised and is running, <see langword="false"/> otherwise.
    /// </summary>
    private bool _deviceInitialised = false;

    /// <summary>
    /// The callback used to process the samples that have been read.
    /// </summary>
    private readonly Interop.ReadDelegate _readCallback;

    /// <summary>
    /// The callback used to process events that have occurred.
    /// </summary>
    private readonly Interop.EventDelegate _eventCallback;

    /// <summary>
    /// The band of the currently tuned frequency.
    /// </summary>
    private RadioBand _currentBand = RadioBand.Unknown;

    /// <summary>
    /// The current gain level.
    /// </summary>
    private uint _currentGainLevel;
    #endregion

    #region Properties
    /// <inheritdoc/>
    public string Name { get; private set; } = string.Empty;

    /// <inheritdoc/>
    public uint SampleRate
    {
        get => _deviceParams != null ? (uint)_deviceParams->DevParams->FsFreq.FsHz : 0;
        set
        {
            _logger.LogInformation($"Setting the sample rate to {value.ToString("N0", Thread.CurrentThread.CurrentCulture)} Hz");

            if (_deviceParams == null)
            {
                _logger.LogError("Unable to set the sample rate");
                return;
            }
            else if (value < 2000000 / MaxDecimationFactor || value > 10000000)
            {
                _logger.LogError("The sample rate is not supported");
                return;
            }

            if (value < 2000000)
            {
                int decimationFactor = 2;
                int calculatedSampleRate = 1000000;

                while (calculatedSampleRate > value)
                {
                    decimationFactor <<= 1;
                    calculatedSampleRate = 2000000 / decimationFactor;
                }

                if (calculatedSampleRate != value)
                {
                    _logger.LogWarning($"The sample rate is not supported, using {calculatedSampleRate.ToString("N0", Thread.CurrentThread.CurrentCulture)} Hz instead");
                }

                value = 2000000;
                _deviceParams->RxChannelA->CtrlParams.Decimation.DecimationFactor = (byte)decimationFactor;
                _deviceParams->RxChannelA->CtrlParams.Decimation.Enable = true;
            }
            else
            {
                _deviceParams->RxChannelA->CtrlParams.Decimation.Enable = false;
            }

            _deviceParams->DevParams->FsFreq.FsHz = value;

            if (value >= 7100000)
            {
                _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw8000;
            }
            else if (value >= 6100000)
            {
                _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw7000;
            }
            else if (value >= 5100000)
            {
                _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw6000;
            }
            else if (value >= 1600000)
            {
                _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw5000;
            }
            else if (value >= 700000)
            {
                _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw1536;
            }
            else if (value >= 400000)
            {
                _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw600;
            }
            else if (value >= 250000)
            {
                _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw300;
            }
            else
            {
                _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw200;
            }

            if (_deviceInitialised && Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Dev_Fs | ReasonForUpdate.Tuner_BwType | ReasonForUpdate.Ctrl_Decimation, ReasonForUpdateExtension1.Ext1_None) != ApiError.Success)
            {
                _logger.LogError("Unable to set the sample rate");
            }
        }
    }

    public TunerType Tuner => TunerType.MSi001;

    /// <inheritdoc/>
    public ulong Frequency
    {
        get => _deviceParams != null ? (ulong)_deviceParams->RxChannelA->TunerParams.RfFreq.RfHz : 0;
        set
        {
            NumberFormatInfo numberFormat = new NumberFormatInfo
            {
                NumberGroupSeparator = "."
            };
            _logger.LogInformation($"Setting the frequency to {value.ToString("N0", numberFormat)} Hz");

            if (_deviceParams == null)
            {
                _logger.LogError("Unable to set the centre frequency");
                return;
            }

            _deviceParams->RxChannelA->TunerParams.RfFreq.RfHz = value;

            if (_deviceInitialised && Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Tuner_Frf, ReasonForUpdateExtension1.Ext1_None) != ApiError.Success)
            {
                _logger.LogError("Unable to set the centre frequency");
                return;
            }

            // Determine the radio band of the frequency
            RadioBand band = RadioBand.Unknown;

            if (value < 60000000)
            {
                band = RadioBand.AM;
            }
            else if (value < 120000000)
            {
                band = RadioBand.VHF;
            }
            else if (value < 250000000)
            {
                band = RadioBand.III;
            }
            else if (value < 420000000)
            {
                band = RadioBand.UHFLower;
            }
            else if (value < 1000000000)
            {
                band = RadioBand.UHFUpper;
            }
            else if (value <= 2000000000)
            {
                band = RadioBand.L;
            }

            // If the band has changed, store the new gain and reapply the current gain level if using manual gain
            if (band != _currentBand)
            {
                _currentBand = band;

                if (GainMode == GainMode.Manual)
                {
                    Gain = Gain;
                }
            }
        }
    }

    /// <inheritdoc/>
    public int FrequencyCorrection
    {
        get => _deviceParams != null ? (int)_deviceParams->DevParams->Ppm : 0;
        set
        {
            _logger.LogInformation($"Setting the frequency correction to {value.ToString("N0", Thread.CurrentThread.CurrentCulture)} ppm");

            _deviceParams->DevParams->Ppm = value;

            if (_deviceInitialised && Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Dev_Ppm, ReasonForUpdateExtension1.Ext1_None) != ApiError.Success)
            {
                _logger.LogError("Unable to set the frequency correction");
            }
        }
    }

    /// <inheritdoc/>
    public bool OffsetTuning
    {
        get => false;
        set => _logger.LogInformation("A change to the offset tuning mode has been requested, but it is not supported by this radio");
    }

    /// <inheritdoc/>
    public DirectSamplingMode DirectSampling
    {
        get => DirectSamplingMode.Off;
        set => _logger.LogInformation("A change to the direct sampling setting has been requested, but it is not supported by this radio");
    }

    /// <inheritdoc/>
    public uint Gain
    {
        get => _currentGainLevel;
        set
        {
            _logger.LogInformation($"Setting the gain to level {value}");

            if (_deviceParams == null)
            {
                _logger.LogError("Unable to set the gain");
            }

            // Get the gain tables for the current device and each band
            byte[] amLnaStates;
            int[] amIfGains;
            byte[] vhfLnaStates;
            int[] vhfIfGains;
            byte[] band3LnaStates;
            int[] band3IfGains;
            byte[] uhfLowerLnaStates;
            int[] uhfLowerIfGains;
            byte[] uhfUpperLnaStates;
            int[] uhfUpperIfGains;
            byte[] lBandLnaStates;
            int[] lBandIfGains;
            switch (_device.HwVer)
            {
                case HardwareVersion.Rsp1:
                default:
                    amLnaStates = GainTables.Rsp1AmLnaStates;
                    amIfGains = GainTables.Rsp1AmIfGains;
                    vhfLnaStates = GainTables.Rsp1VhfLnaStates;
                    vhfIfGains = GainTables.Rsp1VhfIfGains;
                    band3LnaStates = GainTables.Rsp1Band3LnaStates;
                    band3IfGains = GainTables.Rsp1Band3IfGains;
                    uhfLowerLnaStates = GainTables.Rsp1UhfLowerLnaStates;
                    uhfLowerIfGains = GainTables.Rsp1UhfLowerIfGains;
                    uhfUpperLnaStates = GainTables.Rsp1UhfUpperLnaStates;
                    uhfUpperIfGains = GainTables.Rsp1UhfUpperIfGains;
                    lBandLnaStates = GainTables.Rsp1LBandLnaStates;
                    lBandIfGains = GainTables.Rsp1LBandIfGains;
                    break;

                case HardwareVersion.Rsp1A:
                    amLnaStates = GainTables.Rsp1aAmLnaStates;
                    amIfGains = GainTables.Rsp1aAmIfGains;
                    vhfLnaStates = GainTables.Rsp1aVhfLnaStates;
                    vhfIfGains = GainTables.Rsp1aVhfIfGains;
                    band3LnaStates = GainTables.Rsp1Band3LnaStates;
                    band3IfGains = GainTables.Rsp1aBand3IfGains;
                    uhfLowerLnaStates = GainTables.Rsp1aUhfLowerLnaStates;
                    uhfLowerIfGains = GainTables.Rsp1aUhfLowerIfGains;
                    uhfUpperLnaStates = GainTables.Rsp1aUhfUpperLnaStates;
                    uhfUpperIfGains = GainTables.Rsp1aUhfUpperIfGains;
                    lBandLnaStates = GainTables.Rsp1aLBandLnaStates;
                    lBandIfGains = GainTables.Rsp1aLBandIfGains;
                    break;

                case HardwareVersion.Rsp2:
                    amLnaStates = GainTables.Rsp2AmLnaStates;
                    amIfGains = GainTables.Rsp2AmIfGains;
                    vhfLnaStates = GainTables.Rsp2VhfLnaStates;
                    vhfIfGains = GainTables.Rsp2VhfIfGains;
                    band3LnaStates = GainTables.Rsp2Band3LnaStates;
                    band3IfGains = GainTables.Rsp2Band3IfGains;
                    uhfLowerLnaStates = GainTables.Rsp2UhfLowerLnaStates;
                    uhfLowerIfGains = GainTables.Rsp2UhfLowerIfGains;
                    uhfUpperLnaStates = GainTables.Rsp2UhfUpperLnaStates;
                    uhfUpperIfGains = GainTables.Rsp2UhfUpperIfGains;
                    lBandLnaStates = GainTables.Rsp2LBandLnaStates;
                    lBandIfGains = GainTables.Rsp2LBandIfGains;
                    break;

                case HardwareVersion.RspDuo:
                    amLnaStates = GainTables.RspDuoAmLnaStates;
                    amIfGains = GainTables.RspDuoAmIfGains;
                    vhfLnaStates = GainTables.RspDuoVhfLnaStates;
                    vhfIfGains = GainTables.RspDuoVhfIfGains;
                    band3LnaStates = GainTables.RspDuoBand3LnaStates;
                    band3IfGains = GainTables.RspDuoBand3IfGains;
                    uhfLowerLnaStates = GainTables.RspDuoUhfLowerLnaStates;
                    uhfLowerIfGains = GainTables.RspDuoUhfLowerIfGains;
                    uhfUpperLnaStates = GainTables.RspDuoUhfUpperLnaStates;
                    uhfUpperIfGains = GainTables.RspDuoUhfUpperIfGains;
                    lBandLnaStates = GainTables.RspDuoLBandLnaStates;
                    lBandIfGains = GainTables.RspDuoLBandIfGains;
                    break;

                case HardwareVersion.RspDx:
                    amLnaStates = GainTables.RspDxAmLnaStates;
                    amIfGains = GainTables.RspDxAmIfGains;
                    vhfLnaStates = GainTables.RspDxVhfLnaStates;
                    vhfIfGains = GainTables.RspDxVhfIfGains;
                    band3LnaStates = GainTables.RspDxBand3LnaStates;
                    band3IfGains = GainTables.RspDxBand3IfGains;
                    uhfLowerLnaStates = GainTables.RspDxUhfLowerLnaStates;
                    uhfLowerIfGains = GainTables.RspDxUhfLowerIfGains;
                    uhfUpperLnaStates = GainTables.RspDxUhfUpperLnaStates;
                    uhfUpperIfGains = GainTables.RspDxUhfUpperIfGains;
                    lBandLnaStates = GainTables.RspDxLBandLnaStates;
                    lBandIfGains = GainTables.RspDxLBandIfGains;
                    break;
            }

            // Set the gain based on the current band
            switch (_currentBand)
            {
                case RadioBand.AM:
                    _deviceParams->RxChannelA->TunerParams.Gain.LnaState = amLnaStates[value];
                    _deviceParams->RxChannelA->TunerParams.Gain.GrDb = amIfGains[value];
                    break;

                case RadioBand.VHF:
                    _deviceParams->RxChannelA->TunerParams.Gain.LnaState = vhfLnaStates[value];
                    _deviceParams->RxChannelA->TunerParams.Gain.GrDb = vhfIfGains[value];
                    break;

                case RadioBand.III:
                    _deviceParams->RxChannelA->TunerParams.Gain.LnaState = band3LnaStates[value];
                    _deviceParams->RxChannelA->TunerParams.Gain.GrDb = band3IfGains[value];
                    break;

                case RadioBand.UHFLower:
                    _deviceParams->RxChannelA->TunerParams.Gain.LnaState = uhfLowerLnaStates[value];
                    _deviceParams->RxChannelA->TunerParams.Gain.GrDb = uhfLowerIfGains[value];
                    break;

                case RadioBand.UHFUpper:
                    _deviceParams->RxChannelA->TunerParams.Gain.LnaState = uhfUpperLnaStates[value];
                    _deviceParams->RxChannelA->TunerParams.Gain.GrDb = uhfUpperIfGains[value];
                    break;

                case RadioBand.L:
                    _deviceParams->RxChannelA->TunerParams.Gain.LnaState = lBandLnaStates[value];
                    _deviceParams->RxChannelA->TunerParams.Gain.GrDb = lBandIfGains[value];
                    break;
            }

            if (value > GainLevelsSupported || (_deviceInitialised && Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Tuner_Gr, ReasonForUpdateExtension1.Ext1_None) != ApiError.Success))
            {
                _logger.LogError("Unable to set the gain");
                return;
            }

            _currentGainLevel = value;
        }
    }

    /// <inheritdoc/>
    public GainMode GainMode
    {
        get => _deviceParams != null && _deviceParams->RxChannelA->CtrlParams.Agc.Enable != Parameters.Control.AgcControl.AgcDisable ? GainMode.Automatic : GainMode.Manual;
        set
        {
            _logger.LogInformation($"Setting the gain mode to {value}");

            _deviceParams->RxChannelA->CtrlParams.Agc.Enable = value == GainMode.Automatic ? Parameters.Control.AgcControl.Agc50HZ : Parameters.Control.AgcControl.AgcDisable;

            if (_deviceInitialised && Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Ctrl_Agc, ReasonForUpdateExtension1.Ext1_None) != ApiError.Success)
            {
                _logger.LogError("Unable to set the gain mode");
            }
        }
    }

    /// <inheritdoc/>
    public uint GainLevelsSupported => 29;

    /// <inheritdoc/>
    public bool AutomaticGainCorrection
    {
        get => false;
        set => _logger.LogInformation("A change to the automatic gain correction setting has been requested, but it is not supported by this radio");
    }

    /// <inheritdoc/>
    public bool BiasTee
    {
        get
        {
            if (_deviceParams == null)
            {
                return false;
            }

            return _device.HwVer switch
            {
                HardwareVersion.Rsp1A => _deviceParams->RxChannelA->Rsp1aTunerParams.BiasTEnable,
                HardwareVersion.Rsp2 => _deviceParams->RxChannelA->Rsp2TunerParams.BiasTEnable,
                HardwareVersion.RspDuo => _deviceParams->RxChannelA->RspDuoTunerParams.BiasTEnable,
                HardwareVersion.RspDx => _deviceParams->DevParams->RspDxParams.BiasTEnable,
                _ => false
            };
        }
        set
        {
            string state = value ? "on" : "off";

            _logger.LogInformation($"Turning {state} the bias tee");

            if (_deviceParams == null)
            {
                _logger.LogError("Unable to set bias tee");
                return;
            }

            ReasonForUpdate reasonForUpdate = ReasonForUpdate.None;
            ReasonForUpdateExtension1 extendedReasonForUpdate = ReasonForUpdateExtension1.Ext1_None;

            switch (_device.HwVer)
            {
                case HardwareVersion.Rsp1A:
                    _deviceParams->RxChannelA->Rsp1aTunerParams.BiasTEnable = value;
                    reasonForUpdate = ReasonForUpdate.Rsp1a_BiasTControl;
                    break;
                case HardwareVersion.Rsp2:
                    _deviceParams->RxChannelA->Rsp2TunerParams.BiasTEnable = value;
                    reasonForUpdate = ReasonForUpdate.Rsp2_BiasTControl;
                    break;
                case HardwareVersion.RspDuo:
                    _deviceParams->RxChannelA->RspDuoTunerParams.BiasTEnable = value;
                    reasonForUpdate = ReasonForUpdate.RspDuo_BiasTControl;
                    break;
                case HardwareVersion.RspDx:
                    _deviceParams->DevParams->RspDxParams.BiasTEnable = value;
                    extendedReasonForUpdate = ReasonForUpdateExtension1.RspDx_BiasTControl;
                    break;
                default:
                    _logger.LogInformation("Bias tee is not supported by this radio");
                    return;
            }

            if (_deviceInitialised && Interop.Update(_device.Dev, _device.Tuner, reasonForUpdate, extendedReasonForUpdate) != ApiError.Success)
            {
                _logger.LogError("Unable to set bias tee");
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
    /// Initialises a new instance of the <see cref="Radio"/> class.
    /// </summary>
    /// <param name="logger">The logger for the <see cref="Radio"/> class.</param>
    /// <param name="lifetime">The application lifetime service.</param>
    public Radio(ILogger<Radio> logger, IHostApplicationLifetime lifetime)
    {
        // Store a reference to the logger
        _logger = logger;

        // Store a reference to the application lifetime
        _applicationLifetime = lifetime;

        // Set the sample reading callback
        _readCallback = new Interop.ReadDelegate(ProcessSamples);
        _eventCallback = new Interop.EventDelegate(ProcessEvents);
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
        _logger.LogInformation("Starting the SDRplay radio");

        try
        {
            // Open the API
            if (Interop.Open() != ApiError.Success)
            {
                _logger.LogCritical("Unable to open the SDRplay API");
                _applicationLifetime.StopApplication();
                return;
            }

            // Check the API version
            float version = 0f;
            ApiError apiVersionCheck = Interop.ApiVersion(ref version);
            if (apiVersionCheck != ApiError.Success)
            {
                _logger.LogCritical($"Unable to check the SDRplay API version {apiVersionCheck}");
                _applicationLifetime.StopApplication();
                return;
            }
            if (version != SdrPlayApiVersion)
            {
                _logger.LogWarning($"The installed SDRplay API is a different version to the one this application is designed for ({SdrPlayApiVersion}), which may result in compatibility issues");
            }

            // Enable debug features if debug mode is enabled
            if (Program.DebugMode)
            {
                Interop.DebugEnable(IntPtr.Zero, DebugLevel.Verbose);
                Interop.DisableHeartbeat();
            }

            // Lock the API
            if (Interop.LockDeviceApi() != ApiError.Success)
            {
                _logger.LogCritical("Unable to lock the SDRplay API");
                _applicationLifetime.StopApplication();
                return;
            }
            _apiLocked = true;

            // Get the list of the devices
            if (Interop.GetDevices(out Device[]? devices, out uint numberOfDevices, 8) != ApiError.Success)
            {
                _logger.LogCritical("Unable to retrieve the list of SDRplay devices");
                _applicationLifetime.StopApplication();
                return;
            }

            // Check if a SDRplay device is available
            if (numberOfDevices == 0 || devices == null || devices.Length == 0)
            {
                _logger.LogCritical("No SDRplay devices could be found");
                _applicationLifetime.StopApplication();
                return;
            }

            // Get the first device and check if a tuner is available
            Device device = devices[0];
            if (device.Tuner == TunerSelect.Neither)
            {
                _logger.LogCritical("No tuners are available on the SDRplay device");
                _applicationLifetime.StopApplication();
                return;
            }

            // If the device is a RSPduo, set the tuner and the operating mode
            if (device.HwVer == HardwareVersion.RspDuo)
            {
                if (device.Tuner == TunerSelect.Both)
                {
                    device.Tuner = TunerSelect.A;
                }
                device.RspDuoMode = RspDuo.Mode.SingleTuner;
            }

            // Select the device
            if (Interop.SelectDevice(ref device) != ApiError.Success)
            {
                _logger.LogCritical("Unable to select the device");
                _applicationLifetime.StopApplication();
                return;
            }
            _device = device;

            // Set the device name
            Name = $"SDRplay {device.HwVer.ToDeviceModel()} {device.SerNo}";

            // Get the pointer to the device parameters
            if (Interop.GetDeviceParams(device.Dev, out _deviceParams) != ApiError.Success)
            {
                _logger.LogCritical("Unable to get the device parameters");
                _applicationLifetime.StopApplication();
                return;
            }

            // Create the bit depth converter
            if (_device.HwVer == HardwareVersion.Rsp1 || _device.HwVer == HardwareVersion.Rsp2)
            {
                _bitConverter = new(12, 8);
            }
            else
            {
                _bitConverter = new(14, 8);
            }

            // Set the initial state
            BiasTee = false;
            Frequency = DefaultFrequency;
            SampleRate = DefaultSampleRate;
            Gain = 0;
            GainMode = GainMode.Automatic;

            // Initialise the device
            Functions callbacks = new()
            {
                StreamACbFn = _readCallback,
                StreamBCbFn = _readCallback,
                EventCbFn = _eventCallback
            };
            if (Interop.Init(device.Dev, callbacks, IntPtr.Zero) != ApiError.Success)
            {
                _logger.LogCritical("Unable to initialise the device");
                _applicationLifetime.StopApplication();
                return;
            }
            _deviceInitialised = true;

            // Log that the radio has started
            _logger.LogInformation($"Started the radio: {Name}");
        }
        catch (DllNotFoundException)
        {
            _logger.LogCritical("Unable to find the SDRplay API library");
            _applicationLifetime.StopApplication();
        }
        catch (BadImageFormatException)
        {
            _logger.LogCritical("The SDRplay API library or one of its dependencies has been built for the wrong system architecture");
            _applicationLifetime.StopApplication();
        }
        finally
        {
            // Unlock the API
            if (_apiLocked)
            {
                Interop.UnlockDeviceApi();
                _apiLocked = false;
            }
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        // Check that the device has been started
        if (_device.Dev == IntPtr.Zero)
        {
            return;
        }

        // Uninitialise the device
        Interop.Uninit(_device.Dev);
        _deviceInitialised = false;

        // Release the device
        Interop.ReleaseDevice(_device);

        // Stop the API
        Interop.Close();

        // Clear the device
        _device = default;

        // Log that the radio has stopped
        _logger.LogInformation($"The radio has stopped");
    }
    #endregion

    #region Sample handling methods
    /// <summary>
    /// The callback method called by the SDRplay API to provide received samples.
    /// </summary>
    /// <param name="xi">The buffer containing the real samples read from the device.</param>
    /// <param name="xq">The buffer containing the imaginary samples read from the device.</param>
    /// <param name="parameters">Pointer to the stream callback parameters struct.</param>
    /// <param name="numSamples">The number of samples in the buffers.</param>
    /// <param name="reset">Indicates if local buffers should be dropped due to an API has re-initialisation.</param>
    /// <param name="cbContext">The user specific context passed to the callback.</param>
    private void ProcessSamples(short* xi, short* xq, StreamCbParams* parameters, uint numSamples, uint reset, IntPtr cbContext)
    {
        // Create a new buffer to store the interleaved IQ samples to be send to clients
        Span<short> buffer = stackalloc short[(int)numSamples * 2];

        // Copy the samples to the interleaved buffer
        for (int i = 0; i < numSamples; i++)
        {
            buffer[i * 2] = xi[i];
            buffer[(i * 2) + 1] = xq[i];
        }

        // Convert the samples from 16 bit to 8 bit
        Span<byte> convertedBuffer = _bitConverter!.ConvertSamples(buffer);

        // Copy the buffer to a new array of bytes in managed memory
        byte[] bufferArray = convertedBuffer.ToArray();

        // Fire the samples available event
        SamplesAvailable?.Invoke(this, bufferArray);
    }

    /// <summary>
    /// The callback method called by the SDRplay API to indicate that an event has occurred.
    /// </summary>
    /// <param name="eventId">Indicates the type of event that has occurred.</param>
    /// <param name="tuner">Indicates which tuner the event relates to.</param>
    /// <param name="parameters">Pointer to the event callback parameters struct.</param>
    /// <param name="cbContext">The user specific context passed to the callback.</param>
    private void ProcessEvents(Event eventId, TunerSelect tuner, EventParams* parameters, IntPtr cbContext)
    {
        switch (eventId)
        {
            case Event.PowerOverloadChange:
                Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Ctrl_OverloadMsgAck, ReasonForUpdateExtension1.Ext1_None);

                if (parameters->PowerOverloadParams.PowerOverloadChangeType == PowerOverloadCbEventId.OverloadDetected)
                {
                    _logger.LogWarning("ADC power overload has been detected");
                }
                else if (parameters->PowerOverloadParams.PowerOverloadChangeType == PowerOverloadCbEventId.OverloadCorrected)
                {
                    _logger.LogInformation("ADC power overload has been corrected");
                }

                break;

            case Event.DeviceRemoved:
                _logger.LogCritical("The SDRplay device has been removed from the system");
                _applicationLifetime.StopApplication();
                break;
        }
    }
    #endregion
}
