namespace ProtocolShared.Commands.Entity
{
    using MessagePack;

    [MessagePackObject]
    public class PlayerEntity : GameEntity
    {
        [Key(2)] public PlayerState State { get; set; }
        [Key(3)] public float X { get; set; }
        [Key(4)] public float Y { get; set; }
    }
}