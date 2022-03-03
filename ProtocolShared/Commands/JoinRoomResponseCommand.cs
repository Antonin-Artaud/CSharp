namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class JoinRoomResponseCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.JoinRoomResponse;
        
        [Key(0)] public bool Success { get; set; }
    }
}