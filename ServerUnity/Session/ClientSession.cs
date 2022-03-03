namespace ServerUnity.Session
{
    using ServerUnity.Network;
    using ServerUnity.Rooms;

    public class ClientSession
    {
        public uint UserId { get; }
        public ClientConnection ClientConnection { get; }

        public GameRoom CurrentRoom { get; set; }
        
        public ClientSession(uint userId, ClientConnection clientConnection)
        {
            this.UserId = userId;
            this.ClientConnection = clientConnection;
        }
    }
}