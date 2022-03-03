namespace ProtocolShared.Commands.Entity
{
    using MessagePack;

    [MessagePackObject]
    [Union(0, typeof(DoorEntity))]
    [Union(1, typeof(PlayerEntity))]
    public abstract class GameEntity
    {
        [Key(0)] public uint Id { get; set; }
        [Key(1)] public uint OwnerId { get; set; }
    }
}