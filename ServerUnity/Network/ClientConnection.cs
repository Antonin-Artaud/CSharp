namespace ServerUnity.Network
{
    using System;
    using System.Net.Sockets;
    using ServerUnity.Commands;
    using ServerUnity.Session;

    public class ClientConnection : IDisposable
    {
        public uint Id { get; }
        public TcpConnection Connection { get; }
        public CommandManager CommandManager { get; }
        public ClientSession Session { get; set; }

        public ClientConnection(uint id, Socket socket)
        {
            this.Id = id;
            this.Connection = new TcpConnection(socket, this);
            this.CommandManager = new CommandManager(this);
        }

        /// <summary>
        /// leave room
        /// </summary>
        public void Dispose()
        {
            this.Session?.CurrentRoom?.Leave(this.Session.UserId);
        }
    }
}