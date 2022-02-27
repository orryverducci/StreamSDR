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
using StreamSDR.Radios.SdrPlay.Callbacks;
using StreamSDR.Radios.SdrPlay.Hardware;

namespace StreamSDR.Radios.SdrPlay;

/// <summary>
/// Provides access to control and receive samples from a SDRplay radio.
/// </summary>
internal sealed unsafe class Radio : RadioBase
{
    #region Constants
    /// <summary>
    /// The SDRplay API version this has been developed for.
    /// </summary>
    private const float SdrPlayApiVersion = 3.07f;

    /// <summary>
    /// The maximum decimation factor that can be used for sample rates smaller than 2 MHz.
    /// </summary>
    private const uint MaxDecimationFactor = 64;

    /// <summary>
    /// The number of available gain levels.
    /// </summary>
    private const uint AvailableGainLevels = 29;
    #endregion

    #region Private fields
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
    /// The tables of LNA and IF gains to be used for the available gain levels.
    /// </summary>
    private IGainTables? _gainLevels;

    /// <summary>
    /// The current gain level.
    /// </summary>
    private uint _currentGainLevel;
    #endregion

    #region Constructor, finaliser and lifecycle methods
    /// <summary>
    /// Initialises a new instance of the <see cref="Radio"/> class.
    /// </summary>
    /// <param name="logger">The logger for the <see cref="Radio"/> class.</param>
    /// <param name="lifetime">The application lifetime service.</param>
    /// <param name="config">The application configuration.</param>
    public Radio(ILogger<Radio> logger, IHostApplicationLifetime lifetime, IConfiguration config) : base(logger, lifetime, config)
    {
        // Set the sample reading callback
        _readCallback = new Interop.ReadDelegate(ProcessSamples);
        _eventCallback = new Interop.EventDelegate(ProcessEvents);
    }

    /// <inheritdoc/>
    public override void Start()
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

            // Find the ID of the specified device, or use the first one if no device is specified
            int deviceId = 0;
            string serial = _config.GetValue<string>("serial");

            if (serial != null)
            {
                deviceId = Array.FindIndex(devices, device => device.SerNo == serial);

                if (deviceId < 0)
                {
                    _logger.LogCritical($"Could not find a SDRplay device with the serial {serial}");
                    _applicationLifetime.StopApplication();
                    return;
                }
            }

            // Get the device and check if a tuner is available
            Device device = devices[deviceId];
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

            // Set the tuner type
            Tuner = TunerType.MSi001;

            // Set the gain levels to the one for the device
            _gainLevels = device.HwVer switch
            {
                HardwareVersion.Rsp1A => new Rsp1a.GainTables(),
                HardwareVersion.Rsp2 => new Rsp2.GainTables(),
                HardwareVersion.RspDuo => new RspDuo.GainTables(),
                HardwareVersion.RspDx => new RspDx.GainTables(),
                _ => new Rsp1.GainTables()
            };
            GainLevelsSupported = AvailableGainLevels;

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
    public override void Stop()
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

    #region Radio parameter methods
    /// <inheritdoc/>
    protected override uint GetSampleRate() => _deviceParams != null ? (uint)_deviceParams->DevParams->FsFreq.FsHz : 0;

    /// <inheritdoc/>
    protected override int SetSampleRate(uint sampleRate)
    {
        if (_deviceParams == null)
        {
            return int.MinValue;
        }

        // Check the sample rate is in the supported range
        if (sampleRate < 2000000 / MaxDecimationFactor || sampleRate > 10000000)
        {
            _logger.LogError("The sample rate is not supported");
            return int.MinValue + 1;
        }

        // If the sample rate is less than 2 MHz enable decimation and calculate an appropriate factor
        if (sampleRate < 2000000)
        {
            int decimationFactor = 2;
            int calculatedSampleRate = 1000000;

            while (calculatedSampleRate > sampleRate)
            {
                decimationFactor <<= 1;
                calculatedSampleRate = 2000000 / decimationFactor;
            }

            if (calculatedSampleRate != sampleRate)
            {
                _logger.LogWarning($"The sample rate is not supported, using {calculatedSampleRate.ToString("N0", Thread.CurrentThread.CurrentCulture)} Hz instead");
            }

            sampleRate = 2000000;
            _deviceParams->RxChannelA->CtrlParams.Decimation.DecimationFactor = (byte)decimationFactor;
            _deviceParams->RxChannelA->CtrlParams.Decimation.Enable = true;
        }
        else
        {
            _deviceParams->RxChannelA->CtrlParams.Decimation.Enable = false;
        }

        // Set the sample rate
        _deviceParams->DevParams->FsFreq.FsHz = sampleRate;

        // Select the appropriate tuner bandwidth based on the chosen sample rate
        if (sampleRate >= 7100000)
        {
            _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw8000;
        }
        else if (sampleRate >= 6100000)
        {
            _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw7000;
        }
        else if (sampleRate >= 5100000)
        {
            _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw6000;
        }
        else if (sampleRate >= 1600000)
        {
            _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw5000;
        }
        else if (sampleRate >= 700000)
        {
            _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw1536;
        }
        else if (sampleRate >= 400000)
        {
            _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw600;
        }
        else if (sampleRate >= 250000)
        {
            _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw300;
        }
        else
        {
            _deviceParams->RxChannelA->TunerParams.BwType = Parameters.Tuner.BwMhz.Bw200;
        }

        // Return the result of the API update if the device is initialised, otherwise return a successful result
        if (_deviceInitialised)
        {
            return (int)Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Dev_Fs | ReasonForUpdate.Tuner_BwType | ReasonForUpdate.Ctrl_Decimation, ReasonForUpdateExtension1.Ext1_None);
        }
        else
        {
            return 0;
        }
    }

    /// <inheritdoc/>
    protected override ulong GetFrequency() => _deviceParams != null ? (ulong)_deviceParams->RxChannelA->TunerParams.RfFreq.RfHz : 0;

    /// <inheritdoc/>
    protected override int SetFrequency(ulong frequency)
    {
        if (_deviceParams == null)
        {
            return int.MinValue;
        }

        // Set the frequency
        _deviceParams->RxChannelA->TunerParams.RfFreq.RfHz = frequency;

        // Get the result of the API update if the device is initialised, otherwise assume success
        ApiError result = ApiError.Success;
        if (_deviceInitialised)
        {
            result = Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Tuner_Frf, ReasonForUpdateExtension1.Ext1_None);
        }

        // If not successful return the result of the API update
        if (result != ApiError.Success)
        {
            return (int)result;
        }

        // Determine the radio band of the new frequency
        RadioBand band = RadioBand.Unknown;

        if (frequency < 60000000)
        {
            band = RadioBand.AM;
        }
        else if (frequency < 120000000)
        {
            band = RadioBand.VHF;
        }
        else if (frequency < 250000000)
        {
            band = RadioBand.III;
        }
        else if (frequency < 420000000)
        {
            band = RadioBand.UHFLower;
        }
        else if (frequency < 1000000000)
        {
            band = RadioBand.UHFUpper;
        }
        else
        {
            band = RadioBand.L;
        }

        // If the band has changed, store the new band and reapply the current gain level if using manual gain
        if (band != _currentBand)
        {
            _currentBand = band;

            if (GainMode == GainMode.Manual)
            {
                _logger.LogDebug($"Radio band has changed to {band}, reapplying gain");
                SetGain(GetGain());
            }
        }

        // Return a successful result
        return 0;
    }

    /// <inheritdoc/>
    protected override int GetFrequencyCorrection() => _deviceParams != null ? (int)_deviceParams->DevParams->Ppm : 0;

    /// <inheritdoc/>
    protected override int SetFrequencyCorrection(int freqCorrection)
    {
        if (_deviceParams == null)
        {
            return int.MinValue;
        }

        _deviceParams->DevParams->Ppm = freqCorrection;

        if (_deviceInitialised)
        {
            return (int)Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Dev_Ppm, ReasonForUpdateExtension1.Ext1_None);
        }
        else
        {
            return 0;
        }
    }

    /// <inheritdoc/>
    protected override bool GetOffsetTuning() => false;

    /// <inheritdoc/>
    protected override int SetOffsetTuning(bool enabled)
    {
        _logger.LogInformation("A change to the offset tuning mode has been requested, but it is not supported by this radio");
        return 0;
    }

    /// <inheritdoc/>
    protected override DirectSamplingMode GetDirectSampling() => DirectSamplingMode.Off;

    /// <inheritdoc/>
    protected override int SetDirectSampling(DirectSamplingMode mode)
    {
        _logger.LogInformation("A change to the direct sampling setting has been requested, but it is not supported by this radio");
        return 0;
    }

    /// <inheritdoc/>
    protected override uint GetGain() => _currentGainLevel;

    /// <inheritdoc/>
    protected override int SetGain(uint level)
    {
        if (_deviceParams == null)
        {
            return int.MinValue;
        }

        // Set the gain based on the current band
        _deviceParams->RxChannelA->TunerParams.Gain.LnaState = _gainLevels!.LnaStates[_currentBand][level];
        _deviceParams->RxChannelA->TunerParams.Gain.GrDb = _gainLevels!.IfGains[_currentBand][level];

        // Get the result of the API update if the device is initialised, otherwise assume success
        ApiError result = ApiError.Success;
        if (_deviceInitialised)
        {
            result = Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Tuner_Gr, ReasonForUpdateExtension1.Ext1_None);
        }

        // Store the gain level if successful and return the result
        if (result == ApiError.Success)
        {
            _currentGainLevel = level;
            return 0;
        }
        else
        {
            return (int)result;
        }
    }

    /// <inheritdoc/>
    protected override GainMode GetGainMode() => _deviceParams != null && _deviceParams->RxChannelA->CtrlParams.Agc.Enable != Parameters.Control.AgcControl.AgcDisable ? GainMode.Automatic : GainMode.Manual;

    /// <inheritdoc/>
    protected override int SetGainMode(GainMode mode)
    {
        if (_deviceParams == null)
        {
            return int.MinValue;
        }

        _deviceParams->RxChannelA->CtrlParams.Agc.Enable = mode == GainMode.Automatic ? Parameters.Control.AgcControl.Agc50HZ : Parameters.Control.AgcControl.AgcDisable;

        if (_deviceInitialised)
        {
            return (int)Interop.Update(_device.Dev, _device.Tuner, ReasonForUpdate.Ctrl_Agc, ReasonForUpdateExtension1.Ext1_None);
        }
        else
        {
            return 0;
        }
    }

    /// <inheritdoc/>
    protected override bool GetAgc() => false;

    /// <inheritdoc/>
    protected override int SetAgc(bool enabled)
    {
        _logger.LogInformation("A change to the automatic gain correction setting has been requested, but it is not supported by this radio");
        return 0;
    }

    /// <inheritdoc/>
    protected override bool GetBiasTee()
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

    /// <inheritdoc/>
    protected override int SetBiasTee(bool enabled)
    {
        if (_deviceParams == null)
        {
            return int.MinValue;
        }

        ReasonForUpdate reasonForUpdate = ReasonForUpdate.None;
        ReasonForUpdateExtension1 extendedReasonForUpdate = ReasonForUpdateExtension1.Ext1_None;

        switch (_device.HwVer)
        {
            case HardwareVersion.Rsp1A:
                _deviceParams->RxChannelA->Rsp1aTunerParams.BiasTEnable = enabled;
                reasonForUpdate = ReasonForUpdate.Rsp1a_BiasTControl;
                break;
            case HardwareVersion.Rsp2:
                _deviceParams->RxChannelA->Rsp2TunerParams.BiasTEnable = enabled;
                reasonForUpdate = ReasonForUpdate.Rsp2_BiasTControl;
                break;
            case HardwareVersion.RspDuo:
                _deviceParams->RxChannelA->RspDuoTunerParams.BiasTEnable = enabled;
                reasonForUpdate = ReasonForUpdate.RspDuo_BiasTControl;
                break;
            case HardwareVersion.RspDx:
                _deviceParams->DevParams->RspDxParams.BiasTEnable = enabled;
                extendedReasonForUpdate = ReasonForUpdateExtension1.RspDx_BiasTControl;
                break;
            default:
                _logger.LogInformation("Bias tee is not supported by this radio");
                return 0;
        }

        if (_deviceInitialised)
        {
            return (int)Interop.Update(_device.Dev, _device.Tuner, reasonForUpdate, extendedReasonForUpdate);
        }
        else
        {
            return 0;
        }
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

        // Send the samples to the clients
        SendSamplesToClients(bufferArray);
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
