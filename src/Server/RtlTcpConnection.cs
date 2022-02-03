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

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace StreamSDR.Server;

internal class RtlTcpConnection : IDisposable
{
    #region Private fields
    /// <summary>
    /// <see langword="true"/> if <see cref="Dispose"/> has been called, <see langword="false"/> otherwise.
    /// </summary>
    private bool _disposed = false;

    /// <summary>
    /// The TCP client.
    /// </summary>
    private readonly TcpClient _tcpClient;

    /// <summary>
    /// Cancellation token used to signal when the server wants to disconnect.
    /// </summary>
    private readonly CancellationTokenSource _connectionCancellationToken = new();

    /// <summary>
    /// The worker thread used to transmit data to the client.
    /// </summary>
    private readonly Thread _dataTxThread;

    /// <summary>
    /// The worker thread used to receive commands from the client.
    /// </summary>
    private readonly Thread _commandRxThread;

    /// <summary>
    /// The worker thread used to process a disconnection.
    /// </summary>
    private readonly Thread _disconnectThread;

    /// <summary>
    /// A reset event used by the communication threads to indicate a client disconnection.
    /// </summary>
    private readonly ManualResetEvent _disconnectEvent = new(false);

    /// <summary>
    /// The queue of sample buffers waiting to be sent to the client.
    /// </summary>
    private readonly BlockingCollection<byte[]> _buffers = new();
    #endregion

    #region Public properties
    /// <summary>
    /// The IP address of the connected client.
    /// </summary>
    public IPAddress ClientIP { get; private set; }
    #endregion

    #region Events
    /// <summary>
    /// Fired when the client sends a command to the server.
    /// </summary>
    public event EventHandler<RtlTcpCommand>? CommandReceived;

    /// <summary>
    /// Fired when the client disconnects from the server.
    /// </summary>
    public event EventHandler? Disconnected;
    #endregion

    #region Constructor, finaliser and dispose methods
    /// <summary>
    /// Initialises a new instance of the <see cref="RtlTcpConnection"/> class.
    /// </summary>
    /// <param name="tcpClient">The <see cref="TcpClient"/> that is created when the connection is accepted.</param>
    /// <param name="tuner">The type of tuner in the radio.</param>
    /// <param name="tunerGainLevels">The number of levels of gain supported by the radio tuner.</param>
    public RtlTcpConnection(TcpClient tcpClient, Radios.TunerType tuner, uint tunerGainLevels)
    {
        // Store a reference to the TCP client and the IP address of its end point
        _tcpClient = tcpClient;
        ClientIP = _tcpClient.Client.RemoteEndPoint != null ? ((IPEndPoint)_tcpClient.Client.RemoteEndPoint).Address : IPAddress.Any;

        // Create the client communication worker threads
        _dataTxThread = new(DataTransmissionWorker)
        {
            Name = $"{ClientIP}DataTransmissionThread"
        };
        _commandRxThread = new(CommandWorker)
        {
            Name = $"{ClientIP}CommandThread"
        };
        _disconnectThread = new(DisconnectWorker)
        {
            Name = $"{ClientIP}DisconnectThread"
        };

        // Add the dongle information header to the buffer
        SendDongleInfoHeader(tuner, tunerGainLevels);

        // Start the client communication worker threads
        _disconnectThread.Start();
        _dataTxThread.Start();
        _commandRxThread.Start();
    }

    /// <summary>
    /// Finalises the instance of the <see cref="RtlTcpConnection"/> class.
    /// </summary>
    ~RtlTcpConnection() => Dispose(false);

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Dispose"/>
    private void Dispose(bool disposing)
    {
        // Return if already disposed
        if (_disposed)
        {
            return;
        }

        // If being disposed by the managed runtime, release all the managed resources
        if (disposing)
        {
            // Signal to the communication threads that they should stop if they haven't already
            _connectionCancellationToken.Cancel();

            // Unblock the disconnect thread, allowing it to terminate
            _disconnectEvent.Set();

            // Dispose of the queue of sample buffers
            _buffers.Dispose();

            // Wait for the communication threads to stop
            _dataTxThread.Join();
            _commandRxThread.Join();
        }

        // Dispose the tcpClient and all its resources
        _tcpClient.Dispose();

        // Set that dispose has run
        _disposed = true;
    }
    #endregion

    #region Communication handling methods
    /// <summary>
    /// Worker for the data transmission thread. Continually sends sample buffers to the client.
    /// </summary>
    private void DataTransmissionWorker()
    {
        while (!_connectionCancellationToken.IsCancellationRequested)
        {
            try
            {
                // Get a sample buffer to send to the client. This blocks if waiting for buffers.
                byte[] buffer = _buffers.Take(_connectionCancellationToken.Token);

                // Write the buffer to the network stream
                _tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (InvalidOperationException)
            {
                // Stop the connection if an exception is thrown due to the client disconnecting
                _disconnectEvent.Set();
                return;
            }
            catch (Exception ex)
            {
                // Rethrow the exception, unless it's for the buffer take operation being cancelled, which happens on dispose
                if (ex is not OperationCanceledException)
                {
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Worker for the client command thread. Checks if any commands have been received and sends them to the server.
    /// </summary>
    private void CommandWorker()
    {
        while (!_connectionCancellationToken.IsCancellationRequested)
        {
            try
            {
                // Wait for a command to be received from the client
                Span<byte> receivedData = new(new byte[5]);
                int bytesRead = _tcpClient.GetStream().Read(receivedData);

                // If nothing is received stop the connection as the client has disconnected
                if (bytesRead == 0)
                {
                    _disconnectEvent.Set();
                    return;
                }

                // Split the data and convert the values from big endian (network order) to little endian if required
                RtlTcpCommand command = new();
                command.Type = (RtlTcpCommandType)receivedData[0];
                Span<byte> value = receivedData.Slice(1);
                if (BitConverter.IsLittleEndian)
                {
                    value.Reverse();
                }
                command.Value = BitConverter.ToUInt32(value);

                // Fire the command received event
                CommandReceived?.Invoke(this, command);
            }
            catch (System.IO.IOException)
            {
                // Stop the connection if an exception is thrown due to the client disconnecting
                _disconnectEvent.Set();
                return;
            }
        }
    }

    /// <summary>
    /// Worker for the disconnection thread. Handles the disconnection of a client when detected by the communication threads.
    /// </summary>
    private void DisconnectWorker()
    {
        // Wait for a disconnection
        _disconnectEvent.WaitOne();

        // Check that cancellation hasn't already been requested (e.g. by dispose)
        if (_connectionCancellationToken.IsCancellationRequested)
        {
            return;
        }

        // Signal to the communication threads that they should stop if they haven't already
        _connectionCancellationToken.Cancel();

        // Wait for the communication threads to stop
        _dataTxThread.Join();
        _commandRxThread.Join();

        // Fire the disconnected event
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Send the dongle information header to the client.
    /// </summary>
    /// <param name="tuner">The type of tuner in the radio.</param>
    /// <param name="tunerGainLevels">The number of levels of gain supported by the radio tuner.</param>
    private void SendDongleInfoHeader(Radios.TunerType tuner, uint tunerGainLevels)
    {
        // Create an array of 12 bytes representing the dongle information
        byte[] header = new byte[12];

        // Convert the tuner type to one of the rtl-sdr tuner values. If the tuner is not one found in an rtl-sdr, return R820T
        uint tunerType = tuner switch
        {
            Radios.TunerType.Unknown => 0,
            Radios.TunerType.E4000 => 1,
            Radios.TunerType.FC0012 => 2,
            Radios.TunerType.FC0013 => 3,
            Radios.TunerType.FC2580 => 4,
            Radios.TunerType.R828D => 6,
            _ => 5
        };

        // Fill the first 4 bytes with 'RTL0'
        header[0] = 82;
        header[1] = 84;
        header[2] = 76;
        header[3] = 48;

        // Fill the next 4 bytes with the tuner type (5 for R820T)
        header[4] = (byte)((tunerType & 0xFF000000) >> 24);
        header[5] = (byte)((tunerType & 0xFF0000) >> 16);
        header[6] = (byte)((tunerType & 0xFF00) >> 8);
        header[7] = (byte)(tunerType & 0xFF);

        // Fill the final 4 bytes with the number of gain levels
        header[8] = (byte)((tunerGainLevels & 0xFF000000) >> 24);
        header[9] = (byte)((tunerGainLevels & 0xFF0000) >> 16);
        header[10] = (byte)((tunerGainLevels & 0xFF00) >> 8);
        header[11] = (byte)(tunerGainLevels & 0xFF);

        // Add the array of bytes to the buffer
        _buffers.Add(header);
    }

    /// <summary>
    /// Send a sample buffer to the client.
    /// </summary>
    /// <param name="buffer">The buffer to be sent to the client.</param>
    public void SendData(byte[] buffer)
    {
        if (!_disposed)
        {
            _buffers.Add(buffer);
        }
    }
    #endregion
}
