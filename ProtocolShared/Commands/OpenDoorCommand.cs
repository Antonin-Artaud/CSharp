namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class OpenDoorCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.OpenDoor;
        [Key(0)] public uint DoorId { get; set; }
    }
}