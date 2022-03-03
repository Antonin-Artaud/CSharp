namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class JoinRoomRequestCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.JoinRoomRequest;
        [Key(0)] public string RoomId { get; set; }
    }
}