using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketAsyncClient
{
    /// <summary>
    /// Implements the connection logic for the socket client.
    /// </summary>
    internal sealed class SocketClient : IDisposable
    {
        /// <summary>
        /// The socket used to send/receive messages.
        /// </summary>
        private Socket clientSocket;

        /// <summary>
        /// Flag for connected socket.
        /// </summary>
        private bool connected = false;

        /// <summary>
        /// Listener endpoint.
        /// </summary>
        private IPEndPoint hostEndPoint;

        /// <summary>
        /// Signals a connection.
        /// </summary>
        private static AutoResetEvent autoConnectEvent = new AutoResetEvent(false);

        /// <summary>
        /// Create an uninitialized client instance.  
        /// To start the send/receive processing
        /// call the Connect method followed by SendReceive method.
        /// </summary>
        /// <param name="ip">Name of the host where the listener is running.</param>
        /// <param name="port">Number of the TCP port from the listener.</param>
        internal SocketClient(string ip, int port)
        {
            // Instantiates the endpoint and socket.
            this.hostEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            this.clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.clientSocket.Blocking = false;
            this.clientSocket.NoDelay = true;
        }

        /// <summary>
        /// Connect to the host.
        /// </summary>
        /// <returns>True if connection has succeded, else false.</returns>
        internal void Connect()
        {
            var connectArgs = new SocketAsyncEventArgs();

            connectArgs.UserToken = this.clientSocket;
            connectArgs.RemoteEndPoint = this.hostEndPoint;
            connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnConnect);

            this.clientSocket.ConnectAsync(connectArgs);
            autoConnectEvent.WaitOne();

            SocketError errorCode = connectArgs.SocketError;
            if (errorCode != SocketError.Success)
            {
                throw new SocketException((int)errorCode);
            }
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            // Signals the end of connection.
            autoConnectEvent.Set();

            // Set the flag for socket connected.
            this.connected = (e.SocketError == SocketError.Success);
        }

        /// <summary>
        /// Exchange a message with the host.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns>Message sent by the host.</returns>
        internal void Send(string message)
        {
            if (this.connected)
            {
                var nioBuffers = new List<ArraySegment<byte>>();
                for (int i = 0; i < 4000; i++)
                {
                    nioBuffers.Add(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)));
                }

                SocketError errorCode;
                long localWrittenBytes = this.clientSocket.Send(nioBuffers, SocketFlags.None, out errorCode);
                if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock)
                {
                    throw new SocketException((int)errorCode);
                }
                if (errorCode == SocketError.WouldBlock)
                {
                    this.clientSocket.SendAsync(nioBuffers, SocketFlags.None);
                    Console.WriteLine("WouldBlock");
                }
                else
                {
                    Console.WriteLine($"{nioBuffers.Sum(n => n.Array.Length)} {localWrittenBytes}");
                }
            }
            else
            {
                throw new SocketException((int)SocketError.NotConnected);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the instance of SocketClient.
        /// </summary>
        public void Dispose()
        {
            autoConnectEvent.Close();
            if (this.clientSocket.Connected)
            {
                this.clientSocket.Close();
            }
        }

        #endregion
    }
}
