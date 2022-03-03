namespace ProtocolShared
{
    public enum CommandId : byte
    {
        None,
        KeepAliveRequest = 1,
        KeepAliveResponse,
        Authentication,
        AuthenticationSuccess,
        AuthenticationError,
        CreateRoomRequest,
        CreateRoomResponse,
        JoinRoomRequest,
        JoinRoomResponse,
        EntityData,
        EntityRemoved,
        MovePlayer,
        KillPlayer,
        OpenDoor,
    }
}