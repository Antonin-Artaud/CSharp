namespace ClientUnity.Sockets
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using Akatsuki.Server.Network;
    using ProtocolShared;

    public class TcpConnection
    {
        public delegate void OnConnectionSuccessfulDelegate();
        public delegate void OnConnectionFailedDelegate();
        public delegate void OnCommandReceivedDelegate(ICommand command);
        
        private const int ReceiveBufferSize = 1024;
        private const int ReceiveQueueSize = 2048;
        private const int SendQueueSize = 2048;

        private readonly object _locker;
        private readonly SocketAsyncEventArgs _receiveAsyncEventArgs;
        private readonly TcpBuffer _receiveBuffer;
        private readonly SocketAsyncEventArgs _sendAsyncEventArgs;
        private readonly TcpBuffer _sendBuffer;
        private readonly Socket _socket;

        public event OnConnectionSuccessfulDelegate OnConnectionSuccessful;
        public event OnConnectionFailedDelegate OnConnectionFailed;
        public event OnCommandReceivedDelegate OnCommandReceived;

        public bool Disposed { get; private set; }

        public bool Connected => this._socket.Connected;

        public TcpConnection()
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._locker = new object();
            this._receiveAsyncEventArgs = new SocketAsyncEventArgs();
            this._receiveAsyncEventArgs.SetBuffer(new byte[TcpConnection.ReceiveBufferSize], 0, TcpConnection.ReceiveBufferSize);
            this._receiveAsyncEventArgs.Completed += this.OnBytesReceived;
            this._sendAsyncEventArgs = new SocketAsyncEventArgs();
            this._sendAsyncEventArgs.Completed += this.OnBytesSent;
            this._receiveBuffer = new TcpBuffer(TcpConnection.ReceiveQueueSize);
            this._sendBuffer = new TcpBuffer(TcpConnection.SendQueueSize);
        }
        
        public void Connect(string host, ushort port)
        {
            var connectEventArgs = new SocketAsyncEventArgs();

            connectEventArgs.Completed += this.OnConnected;
            connectEventArgs.RemoteEndPoint = new DnsEndPoint(host, port);

            if (!this._socket.ConnectAsync(connectEventArgs))
                this.OnConnected(null, connectEventArgs);
        }

        private void OnConnected(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                this.OnConnectionSuccessful?.Invoke();

                if (!this._socket.ReceiveAsync(this._receiveAsyncEventArgs))
                    this.OnBytesReceived(null, args);
            }
            else
            {
                this.OnConnectionFailed?.Invoke();
            }

            args.Dispose();
        }

        private void Disconnect(int timeout = 0)
        {
            lock (this._locker)
            {
                if (this.Disposed)
                    return;

                this.Disposed = true;
            }

            this._sendAsyncEventArgs.Dispose();
            this._receiveAsyncEventArgs.Dispose();
            this._receiveBuffer.Dispose();
            this._sendBuffer.Dispose();
            this._socket.Close(timeout);
        }

        private void OnBytesReceived(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                do
                {
                    if (args.SocketError != SocketError.Success || args.BytesTransferred == 0)
                    {
                        this.Disconnect();
                        break;
                    }

                    this._receiveBuffer.Write(args.Buffer, args.BytesTransferred);

                    ReadOnlySpan<byte> receiveQueue = this._receiveBuffer.Span;
                    int read = 0;

                    do
                    {
                        var result = this.TryHandlePacket(receiveQueue);

                        if (result < TcpPacket.DecodeResult.Success)
                        {
                            if (result == TcpPacket.DecodeResult.BadData)
                            {
                                this.Disconnect();
                                return;
                            }

                            break;
                        }

                        receiveQueue = receiveQueue.Slice((int) result);
                        read += (int) result;
                    } while (!receiveQueue.IsEmpty);

                    this._receiveBuffer.Remove(read);
                } while (!this.Disposed && !this._socket.ReceiveAsync(args));
            }
            catch (SocketException)
            {
                this.Disconnect();
            }
            catch (Exception exception)
            {
                this.Disconnect();
            }
        }

        private void OnBytesSent(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                this.Disconnect();
                return;
            }

            lock (this._locker)
            {
                if (this.Disposed)
                    return;

                this._sendBuffer.Remove(args.BytesTransferred);

                if (!this._sendBuffer.IsEmpty)
                    this.SendQueuedBytes();
            }
        }

        private void SendQueuedBytes()
        {
            this._sendAsyncEventArgs.SetBuffer(this._sendBuffer.Buffer, 0, this._sendBuffer.Length);

            if (!this._socket.SendAsync(this._sendAsyncEventArgs))
                this.OnBytesSent(null, this._sendAsyncEventArgs);
        }
        
        private TcpPacket.DecodeResult TryHandlePacket(ReadOnlySpan<byte> packet)
        {
            var result = TcpPacket.TryDecode(packet, out var tcpPacket);

            if (result >= TcpPacket.DecodeResult.Success)
            {
                var command = tcpPacket.DecodeCommand();

                if (command == null)
                    return TcpPacket.DecodeResult.BadData;

                this.OnCommandReceived?.Invoke(command);
            }

            return result;
        }

        public void Send(ICommand command) => Send(new TcpPacket(command));
        public void Send(TcpPacket tcpPacket) => Send(tcpPacket.ToArray());

        public void Send(byte[] data)
        {
            lock (this._locker)
            {
                if (this.Disposed)
                    return;

                if (this._sendBuffer.IsEmpty)
                {
                    this._sendBuffer.Write(data, data.Length);
                    this.SendQueuedBytes();
                }
                else
                {
                    this._sendBuffer.Write(data, data.Length);
                }
            }
        }
    }
}