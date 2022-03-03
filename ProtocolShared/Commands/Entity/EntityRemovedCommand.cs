namespace ProtocolShared.Commands.Entity
{
    using MessagePack;

    [MessagePackObject]
    public class EntityRemovedCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.EntityRemoved;
        [Key(0)] public uint EntityId { get; set; }
    }
}