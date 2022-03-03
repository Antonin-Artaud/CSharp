namespace ServerUnity.Rooms
{
    using ProtocolShared.Commands.Entity;
    using ServerUnity.Network;

    public class PlayerInfo
    {
        public uint UserId { get; }
        public ClientConnection ClientConnection { get; }
        public PlayerEntity PlayerEntity { get; }

        public PlayerInfo(uint userId, ClientConnection clientConnection, PlayerEntity playerEntity)
        {
            this.UserId = userId;
            this.ClientConnection = clientConnection;
            this.PlayerEntity = playerEntity;
        }
    }
}