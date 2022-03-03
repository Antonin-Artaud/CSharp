namespace ServerUnity.Network
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.Sockets;

    public static class ClientConnectionManager
    {
        private static ConcurrentDictionary<uint, ClientConnection> _connections = new ConcurrentDictionary<uint, ClientConnection>();
        private static uint _counter;
        
        /// <summary>
        /// Create ClientConnection
        /// </summary>
        /// <param name="socket"></param>
        public static void Create(Socket socket)
        {
            uint id = ClientConnectionManager.CreateId();
            ClientConnection clientConnection = new ClientConnection(id, socket);
            ClientConnectionManager._connections.TryAdd(id, clientConnection);
        }
    
        /// <summary>
        /// delete unsafe clientConnection
        /// </summary>
        /// <param name="connection"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void Delete(ClientConnection connection)
        {
            if (!connection.Connection.Disposed)
                throw new ArgumentException("Connection must be disposed.");

            ClientConnectionManager._connections.TryRemove(connection.Id, out _);
        }

        /// <summary>
        /// increment counter of Id
        /// </summary>
        /// <returns></returns>
        private static uint CreateId()
        {
            return ClientConnectionManager._counter++;
        }
    }
}