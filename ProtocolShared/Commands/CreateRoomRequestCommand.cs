namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class CreateRoomRequestCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.CreateRoomRequest;
        [Key(0)] public string RoomId { get; set; }
    }
}