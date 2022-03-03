namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class KillPlayerCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.KillPlayer;
        [Key(0)] public uint EntityId { get; set; }
    }
}