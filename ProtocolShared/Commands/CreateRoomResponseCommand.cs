namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class CreateRoomResponseCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.CreateRoomResponse;
        [Key(0)] public bool Success { get; set; }
    }
}