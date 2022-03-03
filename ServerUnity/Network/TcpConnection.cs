namespace ServerUnity.Network
{
    using System;
    using System.Net.Sockets;
    using ProtocolShared;

    public class TcpConnection
    {
        private const int ReceiveBufferSize = 1024;
        private const int ReceiveQueueSize = 2048;
        private const int SendQueueSize = 2048;

        private readonly object _locker;
        private readonly Socket _socket;
        private readonly SocketAsyncEventArgs _receiveAsyncEventArgs;
        private readonly SocketAsyncEventArgs _sendAsyncEventArgs;
        private readonly TcpBuffer _receiveBuffer;
        private readonly TcpBuffer _sendBuffer;
        private readonly ClientConnection _clientConnection;

        public bool Disposed { get; private set; }

        public TcpConnection(Socket socket, ClientConnection clientConnection)
        {
            this._socket = socket;
            this._locker = new object();
            this._receiveAsyncEventArgs = new SocketAsyncEventArgs();
            this._receiveAsyncEventArgs.SetBuffer(new byte[TcpConnection.ReceiveBufferSize], 0, TcpConnection.ReceiveBufferSize);
            this._receiveAsyncEventArgs.Completed += this.OnBytesReceived;
            this._sendAsyncEventArgs = new SocketAsyncEventArgs();
            this._sendAsyncEventArgs.Completed += this.OnBytesSent;
            this._receiveBuffer = new TcpBuffer(TcpConnection.ReceiveQueueSize);
            this._sendBuffer = new TcpBuffer(TcpConnection.SendQueueSize);
            this._clientConnection = clientConnection;

            if (!this._socket.ReceiveAsync(this._receiveAsyncEventArgs))
            {
                this.OnBytesReceived(null, this._receiveAsyncEventArgs);
            }
        }
        
        /// <summary>
        /// Disconnect clientConnection
        /// </summary>
        /// <param name="timeout"></param>
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
            this._clientConnection.Dispose();
            
            ClientConnectionManager.Delete(this._clientConnection);
        }
        
        /// <summary>
        /// try to read bytes received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
                        TcpPacket.DecodeResult result = this.TryHandlePacket(receiveQueue);

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
                Console.Error.WriteLine(exception);
                this.Disconnect();
            }
        }
        
        /// <summary>
        /// prepare data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
        /// <summary>
        /// prepare data
        /// </summary>
        private void SendQueuedBytes()
        {
            this._sendAsyncEventArgs.SetBuffer(this._sendBuffer.Memory);

            if (!this._socket.SendAsync(this._sendAsyncEventArgs))
                this.OnBytesSent(null, this._sendAsyncEventArgs);
        }
        
        /// <summary>
        /// Try to decode packet, and create a command
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private TcpPacket.DecodeResult TryHandlePacket(ReadOnlySpan<byte> packet)
        {
            TcpPacket.DecodeResult result = TcpPacket.TryDecode(packet, out var tcpPacket);

            if (result >= TcpPacket.DecodeResult.Success)
            {
                ICommand command = tcpPacket.DecodeCommand();

                if (command == null)
                    return TcpPacket.DecodeResult.BadData;

                this._clientConnection.CommandManager.HandleCommand(command);
            }

            return result;
        }
        
        /// <summary>
        /// convert command to tcp packet
        /// </summary>
        /// <param name="command"></param>
        public void Send(ICommand command) => this.Send(new TcpPacket(command));
        
        /// <summary>
        /// convert tcp packet to bye array
        /// </summary>
        /// <param name="tcpPacket"></param>
        private void Send(TcpPacket tcpPacket) => this.Send(tcpPacket.ToArray());
        
        /// <summary>
        /// Send byte array in buffer and send this buffer
        /// </summary>
        /// <param name="data"></param>
        private void Send(byte[] data)
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