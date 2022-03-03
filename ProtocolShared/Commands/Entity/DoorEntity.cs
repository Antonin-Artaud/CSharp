namespace ProtocolShared.Commands.Entity
{
    using MessagePack;

    [MessagePackObject]
    public class DoorEntity : GameEntity
    {
        [Key(2)] public bool IsOpen { get; set; }
        [Key(3)] public uint DoorId { get; set; }
    }
}