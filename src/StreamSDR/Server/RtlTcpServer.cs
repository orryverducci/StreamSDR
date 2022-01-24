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

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StreamSDR.Server;

/// <summary>
/// Provides a server for SDR client applications using the rtl_tcp protocol.
/// </summary>
internal class RtlTcpServer : IHostedService
{
    #region Private fields
    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// The radio service.
    /// </summary>
    private readonly Radios.IRadio _radio;

    /// <summary>
    /// Cancellation token used to signal when the server should stop.
    /// </summary>
    private readonly CancellationTokenSource _serverCancellationToken = new();

    /// <summary>
    /// The TCP listener that clients connect to.
    /// </summary>
    private TcpListener? _listener;

    /// <summary>
    /// The worker thread used to process client connections.
    /// </summary>
    private readonly Thread _listenerThread;

    /// <summary>
    /// A list of the current connections to clients.
    /// </summary>
    private readonly List<RtlTcpConnection> _connections = new();

    /// <summary>
    /// The object to lock on to when using the list of connections.
    /// </summary>
    private readonly object _connectionsLock = new();
    #endregion

    #region Constructor and lifetime methods
    /// <summary>
    /// Initialises a new instance of the <see cref="RtlTcpServer"/> class.
    /// </summary>
    /// <param name="logger">The logger provided by the host.</param>
    public RtlTcpServer(ILogger<RtlTcpServer> logger, Radios.IRadio radio)
    {
        // Store a reference to the logger
        _logger = logger;

        // Store a reference to the radio service and handle the samples available event
        _radio = radio;
        radio.SamplesAvailable += RadioSamplesAvailable;

        // Create the TCP listener worker thread
        _listenerThread = new(ListenerWorker)
        {
            Name = "TCPListenerThread"
        };
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Log that the server is starting
        _logger.LogInformation("Starting TCP server on port 1234");

        // Start the radio
        _radio.Start();

        if (!cancellationToken.IsCancellationRequested)
        {
            // Set up the TCP listener on port 1234
            _listener = new(IPAddress.Any, 1234);

            // Start the TCP listener
            _listener.Start();

            // Start the listener worker thread
            _listenerThread.Start();

            // Log and return that the server has started
            _logger.LogInformation("TCP server is now running");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Check that the listener had been started
        if (_listener == null)
        {
            return;
        }

        // Log that the server is stopping
        _logger.LogInformation("Stopping TCP server");

        // Indicate to the listener worker thread that it needs to stop, and stop the TCP listener
        _serverCancellationToken.Cancel();
        _listener?.Stop();

        // Wait until the listener thread stops
        await Task.Run(() => _listenerThread.Join(), cancellationToken);

        // Stop the radio
        _radio.Stop();
        _radio.Dispose();

        // Stop each of the running connections
        foreach (RtlTcpConnection connection in _connections)
        {
            await Task.Run(() => connection.Dispose());
        }

        // Log and return that the server has stopped
        _logger.LogInformation("TCP server has stopped");
    }
    #endregion

    #region Connection handling methods
    /// <summary>
    /// Worker for the listener thead. Continuously accepts and processes new connections until the server is stopped.
    /// </summary>
    private void ListenerWorker()
    {
        while (!_serverCancellationToken.IsCancellationRequested)
        {
            try
            {
                // Wait for a connection and accept it
                TcpClient client = _listener!.AcceptTcpClient();

                // Create a new connection instance to handle communication to the client, and add it to the list of connections
                RtlTcpConnection connection = new(client, _radio.Tuner, _radio.GainLevelsSupported);
                connection.CommandReceived += CommandReceived;
                connection.Disconnected += ClientDisconnected;
                lock (_connectionsLock)
                {
                    _connections.Add(connection);
                }

                // Log the connection
                _logger.LogInformation($"Connected to {connection.ClientIP}");

            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.Interrupted)
                {
                    _logger.LogError(ex, "The TCP listener encountered an error");
                }
            }
        }
    }

    /// <summary>
    /// Event handler for commands received from clients.
    /// </summary>
    /// <param name="sender">The sending object.</param>
    /// <param name="command">A command received as a tuple containing the command type and the value.</param>
    private void CommandReceived(object? sender, RtlTcpCommand command)
    {
        switch (command.Type)
        {
            case RtlTcpCommandType.Tune:
                _radio.Frequency = command.Value;
                break;
            case RtlTcpCommandType.SampleRate:
                _radio.SampleRate = command.Value;
                break;
            case RtlTcpCommandType.GainMode:
                _radio.GainMode = command.Value == 1 ? Radios.GainMode.Manual : Radios.GainMode.Automatic;
                break;
            case RtlTcpCommandType.FrequencyCorrection:
                _radio.FrequencyCorrection = unchecked((int)command.Value);
                break;
            case RtlTcpCommandType.GainCorrection:
                _radio.AutomaticGainCorrection = command.Value == 1;
                break;
            case RtlTcpCommandType.DirectSampling:
                switch (command.Value)
                {
                    case 1:
                        _radio.DirectSampling = Radios.DirectSamplingMode.IBranch;
                        break;
                    case 2:
                        _radio.DirectSampling = Radios.DirectSamplingMode.QBranch;
                        break;
                    default:
                        _radio.DirectSampling = Radios.DirectSamplingMode.Off;
                        break;
                }
                break;
            case RtlTcpCommandType.OffsetTuning:
                _radio.OffsetTuning = command.Value == 1;
                break;
            case RtlTcpCommandType.TunerGainByIndex:
                _radio.Gain = command.Value;
                break;
            case RtlTcpCommandType.BiasTee:
                _radio.BiasTee = command.Value == 1;
                break;
            default:
                _logger.LogWarning($"The rtp_tcp client has sent an unsupported command (command 0x{command.Type.ToString("X2")}, value {command.Value})");
                break;
        }
    }

    /// <summary>
    /// Event handler for client disconnections. Disposes the connection and removes it from the list of connections.
    /// </summary>
    /// <param name="sender">The sending object.</param>
    /// <param name="e">The event arguments.</param>
    private void ClientDisconnected(object? sender, EventArgs e)
    {
        if (sender != null)
        {
            RtlTcpConnection connection = (RtlTcpConnection)sender;
            lock (_connectionsLock)
            {
                _connections.Remove(connection);
            }

            // Dispose of the connection in a thread from the thread pool. This prevents the connection
            // worker thread waiting on itself to stop, which never happens
            Task.Run(() => connection.Dispose());

            // Log the disconnection
            _logger.LogInformation($"Disconnected from {connection.ClientIP}");
        }
    }
    #endregion

    #region Radio methods
    /// <summary>
    /// Event handler for the reception of samples. Sends the buffer of samples to the connected clients.
    /// </summary>
    /// <param name="sender">The sending object.</param>
    /// <param name="e">The received buffer of samples.</param>
    private void RadioSamplesAvailable(object? sender, byte[] buffer)
    {
        lock (_connectionsLock)
        {
            foreach (RtlTcpConnection connection in _connections)
            {
                connection.SendData(buffer);
            }
        }
    }
    #endregion
}
