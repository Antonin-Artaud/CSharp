namespace ServerUnity.Network
{
    using System.Net;
    using System.Net.Sockets;

    public static class TcpServer
    {
        private static Socket _socket;
        private static SocketAsyncEventArgs _connectAsyncEventArgs;
        
        /// <summary>
        /// create a new instance of socket async
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void Listen(string ip, ushort port)
        {
            TcpServer._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TcpServer._socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            TcpServer._socket.Listen(100);
            TcpServer._connectAsyncEventArgs = new SocketAsyncEventArgs();
            TcpServer._connectAsyncEventArgs.Completed += TcpServer.OnAccept;

            if (!TcpServer._socket.AcceptAsync(TcpServer._connectAsyncEventArgs))
            {
                TcpServer.OnAccept(null, TcpServer._connectAsyncEventArgs);
            }
        }

        /// <summary>
        /// accept new connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="connectEventArgs"></param>
        private static void OnAccept(object sender, SocketAsyncEventArgs connectEventArgs)
        {
            do
            {
                if (connectEventArgs.SocketError == SocketError.Success)
                    TcpServer.CreateConnection(connectEventArgs.AcceptSocket);

                connectEventArgs.AcceptSocket = null;
            } while (!TcpServer._socket.AcceptAsync(connectEventArgs));
        }

        /// <summary>
        /// create a new clientConnection
        /// </summary>
        /// <param name="socket"></param>
        private static void CreateConnection(Socket socket)
        {
            ClientConnectionManager.Create(socket);
        }
    }
}