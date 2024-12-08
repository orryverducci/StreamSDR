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

namespace StreamSDR.Radios.Dummy;

/// <summary>
/// Provides a dummy radio, simulating a radio receiving white noise samples. Used to test the software without a real radio.
/// </summary>
internal sealed class Radio : RadioBase
{
    #region Constants
    /// <summary>
    /// The interval between sample buffers in milliseconds.
    /// </summary>
    private const int SampleInterval = 50;

    /// <summary>
    /// The supported levels of gain.
    /// </summary>
    private const int GainLevels = 29;
    #endregion

    #region Private fields
    /// <summary>
    /// The worker thread used to generate samples.
    /// </summary>
    private readonly Thread _generatorThread;

    /// <summary>
    /// Random number generator.
    /// </summary>
    private readonly Random _random = new();

    /// <summary>
    /// The size of the buffers to be sent to clients.
    /// </summary>
    private int _bufferSize;

    /// <summary>
    /// The gain to be applied to the samples.
    /// </summary>
    private double _gain;

    /// <summary>
    /// The gain level set by the client.
    /// </summary>
    private uint _gainLevel;

    /// <summary>
    /// The object to lock on to when using the gain.
    /// </summary>
    private readonly Lock _gainLock = new();

    /// <summary>
    /// If the dummy radio is running.
    /// </summary>
    private bool _running = false;
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
        _generatorThread = new(SampleGenerator)
        {
            Name = "GeneratorThread"
        };
    }

    /// <inheritdoc/>
    public override void Start()
    {
        // Log that the radio is starting
        _logger.LogInformation("Starting the dummy radio");

        // Set the device properties
        Name = "Dummy SDR";
        Tuner = TunerType.R820T;
        GainLevelsSupported = GainLevels;

        // Set the initial sample rate
        _logger.LogDebug("Setting the initial state for the radio");
        SampleRate = DefaultSampleRate;

        // Start the generator
        _generatorThread.Start();
        _running = true;

        // Log that the radio has started
        _logger.LogInformation($"Started the radio: {Name}");
    }

    /// <inheritdoc/>
    public override void Stop()
    {
        // Check that the device has been started
        if (!_running)
        {
            return;
        }

        // Log that the radio is stopping
        _logger.LogInformation($"Stopping the radio ({Name})");

        // Stop generating samples
        _running = false;
        _generatorThread.Join();
        _logger.LogDebug("Sample generating thread stopped");

        // Clear the device name
        Name = string.Empty;

        // Log that the radio has stopped
        _logger.LogInformation($"The radio has stopped");
    }
    #endregion

    #region Radio parameter methods
    /// <inheritdoc/>
    protected override uint GetSampleRate() => (uint)(_bufferSize * SampleInterval);

    /// <inheritdoc/>
    protected override int SetSampleRate(uint sampleRate)
    {
        // Calculate the buffer size needed to simulate the sample rate
        _bufferSize = (int)(sampleRate / 1000 * SampleInterval) * 2;

        // Return success
        return 0;
    }

    /// <inheritdoc/>
    protected override uint GetGain() => _gainLevel;

    /// <inheritdoc/>
    protected override int SetGain(uint level)
    {
        lock (_gainLock)
        {
            _gainLevel = level;

            // Calculate linear gain, and then the logarithmic gain from that
            double linearGain = 1d / (GainLevels - 1) * _gainLevel;
            _gain = (Math.Pow(10, linearGain) - 1) / 9;

            // Return success
            return 0;
        }
    }
    #endregion

    #region Sample handling methods
    /// <summary>
    /// Worker for the sample generator thead. Generates white noise samples at the interval required to simulate the specified bandwidth.
    /// </summary>
    private void SampleGenerator()
    {
        _logger.LogDebug("Sample generating thread started");

        while (_running)
        {
            // Create a buffer
            byte[] buffer = new byte[_bufferSize];

            // Generate samples of gaussian white noise
            lock (_gainLock)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    double u1 = _random.NextDouble();
                    double u2 = _random.NextDouble();

                    double r = Math.Sqrt(-2 * Math.Log(u1));
                    double theta = 2 * Math.PI * u2;

                    double sample = Math.Clamp(r * Math.Cos(theta) * _gain, -1, 1);

                    buffer[i] = (byte)Math.Round((sample * 127.5) + 128);
                }
            }

            // Send the samples to the clients
            SendSamplesToClients(buffer);

            // Wait for the next sample period
            Thread.Sleep(SampleInterval);
        }
    }
    #endregion
}

