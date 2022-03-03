namespace ProtocolShared.Commands.Entity
{
    using MessagePack;

    [MessagePackObject]
    public class EntityDataCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.EntityData;
        [Key(1)] public GameEntity Entity { get; set; }
    }
}