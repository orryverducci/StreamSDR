using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace StreamSDR.Server
{
    internal class RtlTcpConnection : IDisposable
    {
        #region Private fields
        /// <summary>
        /// <see langword="true"/> if Dispose() has been called, <see langword="false"/> otherwise.
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
        /// The worker thread used to communicate with the client.
        /// </summary>
        private readonly Thread _communicationThread;

        /// <summary>
        /// The queue of sample buffers waiting to be sent to the client.
        /// </summary>
        private readonly BlockingCollection<byte[]> _buffers = new();
        #endregion

        #region Public properties
        /// <summary>
        /// The IP address of the connected client.
        /// </summary>
        public IPAddress? ClientIP
        {
            get
            {
                if (_tcpClient.Client.RemoteEndPoint != null)
                {
                    return ((IPEndPoint)_tcpClient.Client.RemoteEndPoint).Address;
                }
                else
                {
                    return IPAddress.Any;
                }
            }
        }
        #endregion

        #region Events
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
        public RtlTcpConnection(TcpClient tcpClient)
        {
            // Store a reference to the TCP client and its network stream
            _tcpClient = tcpClient;

            // Create the client communication worker thread
            _communicationThread = new(CommunicationWorker)
            {
                Name = "ClientCommunicationThread"
            };

            // Start the client communication worker thread
            _communicationThread.Start();
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
                // Signal to the communication thread that it should stop
                _connectionCancellationToken.Cancel();

                // Dispose of the queue of sample buffers
                _buffers.Dispose();

                // Wait until the communication thread stops
                _communicationThread.Join();
            }

            // Dispose the tcpClient and all its resources
            _tcpClient.Dispose();

            // Set that dispose has run
            _disposed = true;
        }
        #endregion

        #region Communication handling methods
        /// <summary>
        /// Worker for the communication thead. Continually sends sample buffers to the client.
        /// </summary>
        private void CommunicationWorker()
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
                catch (Exception ex)
                {
                    if (!(ex is OperationCanceledException))
                    {
                        Disconnected?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Send a sample buffer to the client.
        /// </summary>
        /// <param name="buffer">The buffer to be sent to the client.</param>
        public void SendData(byte[] buffer) => _buffers.Add(buffer);
        #endregion
    }
}
