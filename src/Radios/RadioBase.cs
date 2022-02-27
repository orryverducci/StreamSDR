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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StreamSDR.Radios;

/// <summary>
/// Provides the base class to be used by individual radio implementations.
/// </summary>
internal abstract class RadioBase : IDisposable
{
    #region Constants
    /// <summary>
    /// The default frequency (100 MHz)
    /// </summary>
    protected const uint DefaultFrequency = 100000000;

    /// <summary>
    /// The default sample rate (2.048 MHz)
    /// </summary>
    protected const uint DefaultSampleRate = 2048000;
    #endregion

    #region Private and protected fields
    /// <summary>
    /// <see langword="true"/> if Dispose() has been called, <see langword="false"/> otherwise.
    /// </summary>
    private bool _disposed = false;

    /// <summary>
    /// The logger.
    /// </summary>
    protected readonly ILogger _logger;

    /// <summary>
    /// The application lifetime service.
    /// </summary>
    protected readonly IHostApplicationLifetime _applicationLifetime;

    /// <summary>
    /// The application configuration.
    /// </summary>
    protected readonly IConfiguration _config;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the device name.
    /// </summary>
    public string Name { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the type of tuner in the device.
    /// </summary>
    public TunerType Tuner { get; protected set; } = TunerType.Unknown;

    /// <summary>
    /// Gets or sets the sample rate the device is operating at in Hertz.
    /// </summary>
    public uint SampleRate
    {
        get
        {
            _logger.LogDebug("Getting the sample rate");

            uint sampleRate = GetSampleRate();
            _logger.LogDebug($"Retrieved sample rate: {sampleRate}");

            return sampleRate;
        }
        set
        {
            _logger.LogInformation($"Setting the sample rate to {value.ToString("N0", Thread.CurrentThread.CurrentCulture)} Hz");

            int result = SetSampleRate(value);
            _logger.LogDebug($"SetSampleRate result: {result}");

            if (result != 0)
            {
                _logger.LogError("Unable to set the sample rate");
            }
        }
    }

    /// <summary>
    /// Gets or sets the centre frequency the device is tuned to in Hertz.
    /// </summary>
    public ulong Frequency
    {
        get
        {
            _logger.LogDebug("Getting the frequency");

            ulong frequency = GetFrequency();

            NumberFormatInfo numberFormat = new NumberFormatInfo
            {
                NumberGroupSeparator = "."
            };
            _logger.LogDebug($"Retrieved frequency: {frequency.ToString("N0", numberFormat)} Hz");

            return frequency;
        }
        set
        {
            NumberFormatInfo numberFormat = new NumberFormatInfo
            {
                NumberGroupSeparator = "."
            };
            _logger.LogInformation($"Setting the frequency to {value.ToString("N0", numberFormat)} Hz");

            int result = SetFrequency(value);
            _logger.LogDebug($"SetFrequency result: {result}");

            if (result != 0)
            {
                _logger.LogError("Unable to set the centre frequency");
            }
        }
    }

    /// <summary>
    /// Gets or sets the tuner frequency correction in parts per million (PPM).
    /// </summary>
    public int FrequencyCorrection
    {
        get
        {
            _logger.LogDebug("Getting the frequency correction");

            int freqCorrection = GetFrequencyCorrection();
            _logger.LogDebug($"Retrieved frequency correction: {freqCorrection.ToString("N0", Thread.CurrentThread.CurrentCulture)} ppm");

            return freqCorrection;
        }
        set
        {
            _logger.LogInformation($"Setting the frequency correction to {value.ToString("N0", Thread.CurrentThread.CurrentCulture)} ppm");

            int result = SetFrequencyCorrection(value);
            _logger.LogDebug($"SetFrequencyCorrection result: {result}");

            if (result != 0)
            {
                _logger.LogError("Unable to set the frequency correction");
            }
        }
    }

    /// <summary>
    /// Gets or sets if offset tuning is enabled for zero IF tuners.
    /// </summary>
    public bool OffsetTuning
    {
        get
        {
            _logger.LogDebug("Getting the offset tuning status");

            bool enabled = GetOffsetTuning();
            string state = enabled ? "on" : "off";
            _logger.LogDebug($"Retrieved offset tuning status: {state}");

            return enabled;
        }
        set
        {
            string state = value ? "on" : "off";
            _logger.LogInformation($"Turning {state} offset tuning");

            int result = SetOffsetTuning(value);
            _logger.LogDebug($"SetOffsetTuning result: {result}");

            if (result != 0)
            {
                _logger.LogError("Unable to set offset tuning");
            }
        }
    }

    /// <summary>
    /// Gets or sets the direct sampling mode.
    /// </summary>
    public DirectSamplingMode DirectSampling
    {
        get
        {
            _logger.LogDebug("Getting the direct sampling mode");

            DirectSamplingMode mode = GetDirectSampling();
            _logger.LogDebug($"Retrieved direct sampling mode: {mode}");

            return mode;
        }
        set
        {
            _logger.LogInformation($"Setting direct sampling to {value}");

            int result = SetDirectSampling(value);
            _logger.LogDebug($"SetDirectSampling result: {result}");

            if (result != 0)
            {
                _logger.LogError("Unable to set the direct sampling mode");
            }
        }
    }

    /// <summary>
    /// Gets or sets the current level of gain of the tuner.
    /// </summary>
    public uint Gain
    {
        get
        {
            _logger.LogDebug("Getting the gain level");

            uint level = GetGain();
            _logger.LogDebug($"Retrieved gain level: {level}");

            return level;
        }
        set
        {
            _logger.LogInformation($"Setting the gain to level {value}");

            if (GainMode == GainMode.Automatic)
            {
                _logger.LogError("Unable to set a gain level while set to automatic gain mode");
                return;
            }

            if (value >= GainLevelsSupported)
            {
                _logger.LogError("The gain level is outside the available range");
                return;
            }

            int result = SetGain(value);
            _logger.LogDebug($"SetGain result: {result}");

            if (result != 0)
            {
                _logger.LogError("Unable to set the gain");
            }
        }
    }

    /// <summary>
    /// Gets or sets the mode in which the radio's gain is operating.
    /// </summary>
    public GainMode GainMode
    {
        get
        {
            _logger.LogDebug("Getting the gain mode");

            GainMode mode = GetGainMode();
            _logger.LogDebug($"Retrieved gain mode: {mode}");

            return mode;
        }
        set
        {
            _logger.LogInformation($"Setting the gain mode to {value}");

            int result = SetGainMode(value);
            _logger.LogDebug($"SetGainMode result: {result}");

            if (result != 0)
            {
                _logger.LogError("Unable to set the gain mode");
            }

            // If switching to manual gain, reset the manual gain value
            if (value == GainMode.Manual)
            {
                uint gain = Gain;

                _logger.LogDebug($"Resetting to gain to level {gain}");

                SetGain(gain);
            }
        }
    }

    /// <summary>
    /// Gets the number of levels of gain that are supported by the tuner.
    /// </summary>
    public uint GainLevelsSupported { get; protected set; }

    /// <summary>
    /// Gets or sets if automatic gain correction is enabled.
    /// </summary>
    public bool AutomaticGainCorrection
    {
        get
        {
            _logger.LogDebug("Getting the RTL AGC status");

            bool enabled = GetAgc();
            string state = enabled ? "on" : "off";
            _logger.LogDebug($"Retrieved RTL AGC status: {state}");

            return enabled;
        }
        set
        {
            string state = value ? "on" : "off";
            _logger.LogInformation($"Setting the RTL AGC to {state}");

            int result = SetAgc(value);
            _logger.LogDebug($"SetAgc result: {result}");

            if (result != 0)
            {
                _logger.LogError("Unable to set the RTL AGC");
            }
        }
    }

    /// <summary>
    /// Gets or sets if the bias tee has been enabled.
    /// </summary>
    public bool BiasTee
    {
        get
        {
            _logger.LogDebug("Getting the bias tee status");

            bool enabled = GetBiasTee();
            string state = enabled ? "on" : "off";
            _logger.LogDebug($"Retrieved bias tee status: {state}");

            return enabled;
        }
        set
        {
            string state = value ? "on" : "off";
            _logger.LogInformation($"Turning {state} the bias tee");

            int result = SetBiasTee(value);
            _logger.LogDebug($"SetBiasTee result: {result}");

            if (result != 0)
            {
                _logger.LogError("Unable to set bias tee");
            }
        }
    }
    #endregion

    #region Events
    /// <summary>
    /// Event fired when samples have been received from the device, provided as an array of bytes containing interleaved IQ samples.
    /// </summary>
    public event EventHandler<byte[]>? SamplesAvailable;
    #endregion

    #region Constructor, finaliser and lifecycle methods
    /// <summary>
    /// Initialises a new instance of the <see cref="RadioBase"/> class.
    /// </summary>
    /// <param name="logger">The logger for the <see cref="RadioBase"/> class.</param>
    /// <param name="lifetime">The application lifetime service.</param>
    /// <param name="config">The application configuration.</param>
    public RadioBase(ILogger<RadioBase> logger, IHostApplicationLifetime lifetime, IConfiguration config)
    {
        // Store a reference to the logger
        _logger = logger;

        // Store a reference to the application lifetime
        _applicationLifetime = lifetime;

        // Store a reference to the application config
        _config = config;
    }

    /// <summary>
    /// Finalises the instance of the <see cref="RadioBase"/> class.
    /// </summary>
    ~RadioBase() => Dispose();

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

    /// <summary>
    /// Starts the device.
    /// </summary>
    public abstract void Start();

    /// <summary>
    /// Stops the device.
    /// </summary>
    public abstract void Stop();
    #endregion

    #region Radio parameter methods
    /// <summary>
    /// Gets the sample rate the device is operating at in Hertz.
    /// </summary>
    /// <returns>The sample rate the device is operating at in Hertz, or 0 if there was an error.</returns>
    protected abstract uint GetSampleRate();

    /// <summary>
    /// Sets the sample rate the device is operating at in Hertz.
    /// </summary>
    /// <param name="sampleRate">The sample rate to be used.</param>
    /// <returns>The error code returned by the device API. Returns 0 if successful.</returns>
    protected abstract int SetSampleRate(uint sampleRate);

    /// <summary>
    /// Gets the centre frequency the device is tuned to in Hertz.
    /// </summary>
    /// <returns>The centre frequency the device is tuned to in Hertz, or 0 if there was an error.</returns>
    protected abstract ulong GetFrequency();

    /// <summary>
    /// Sets the centre frequency the device is tuned to in Hertz.
    /// </summary>
    /// <param name="frequency">The centre frequency to tune the device to.</param>
    /// <returns>The error code returned by the device API. Returns 0 if successful.</returns>
    protected abstract int SetFrequency(ulong frequency);

    /// <summary>
    /// Gets the tuner frequency correction in parts per million (PPM).
    /// </summary>
    /// <returns>The tuner frequency correction in parts per million (PPM), or 0 if there was an error.</returns>
    protected abstract int GetFrequencyCorrection();

    /// <summary>
    /// Sets the tuner frequency correction in parts per million (PPM).
    /// </summary>
    /// <param name="freqCorrection">The tuner frequency correction to be used.</param>
    /// <returns>The error code returned by the device API. Returns 0 if successful.</returns>
    protected abstract int SetFrequencyCorrection(int freqCorrection);

    /// <summary>
    /// Gets if offset tuning is enabled for zero IF tuners.
    /// </summary>
    /// <returns><see langword="true"/> if offset tuning is enabled, <see langword="false"/> otherwise.</returns>
    protected abstract bool GetOffsetTuning();

    /// <summary>
    /// Sets if offset tuning is enabled for zero IF tuners.
    /// </summary>
    /// <param name="enabled">The offset tuning state to be used.</param>
    /// <returns>The error code returned by the device API. Returns 0 if successful.</returns>
    protected abstract int SetOffsetTuning(bool enabled);

    /// <summary>
    /// Gets the direct sampling mode.
    /// </summary>
    /// <returns>The currently set <see cref="DirectSamplingMode"/>.</returns>
    protected abstract DirectSamplingMode GetDirectSampling();

    /// <summary>
    /// Sets the direct sampling mode.
    /// </summary>
    /// <param name="mode">The <see cref="DirectSamplingMode"/> to be used.</param>
    /// <returns>The error code returned by the device API. Returns 0 if successful.</returns>
    protected abstract int SetDirectSampling(DirectSamplingMode mode);

    /// <summary>
    /// Gets the current level of gain of the tuner.
    /// </summary>
    /// <returns>The level of gain the tuner is using.</returns>
    protected abstract uint GetGain();

    /// <summary>
    /// Sets the current level of gain of the tuner.
    /// </summary>
    /// <param name="level">The tuner gain level to be used.</param>
    /// <returns>The error code returned by the device API. Returns 0 if successful.</returns>
    protected abstract int SetGain(uint level);

    /// <summary>
    /// Gets the mode in which the radio's gain is operating.
    /// </summary>
    /// <returns>The currently set <see cref="Radios.GainMode"/>.</returns>
    protected abstract GainMode GetGainMode();

    /// <summary>
    /// Sets the mode in which the radio's gain is operating.
    /// </summary>
    /// <param name="mode">The <see cref="Radios.GainMode"/> to be used.</param>
    /// <returns>The error code returned by the device API. Returns 0 if successful.</returns>
    protected abstract int SetGainMode(GainMode mode);

    /// <summary>
    /// Gets if automatic gain correction is enabled.
    /// </summary>
    /// <returns><see langword="true"/> if automatic gain correction, <see langword="false"/> otherwise.</returns>
    protected abstract bool GetAgc();

    /// <summary>
    /// Sets if automatic gain correction is enabled.
    /// </summary>
    /// <param name="enabled">The automatic gain correction state to be used.</param>
    /// <returns>The error code returned by the device API. Returns 0 if successful.</returns>
    protected abstract int SetAgc(bool enabled);

    /// <summary>
    /// Gets if the bias tee has been enabled.
    /// </summary>
    /// <returns><see langword="true"/> if the bias tee is enabled, <see langword="false"/> otherwise.</returns>
    protected abstract bool GetBiasTee();

    /// <summary>
    /// Sets if the bias tee has been enabled.
    /// </summary>
    /// <param name="enabled">The bias tee state to be used.</param>
    /// <returns>The error code returned by the device API. Returns 0 if successful.</returns>
    protected abstract int SetBiasTee(bool enabled);
    #endregion

    #region Sample handling methods
    /// <summary>
    /// Sends a buffer of samples to the clients.
    /// </summary>
    /// <param name="buffer">An array of bytes containing the samples.</param>
    protected void SendSamplesToClients(byte[] buffer) => SamplesAvailable?.Invoke(this, buffer);
    #endregion
}
