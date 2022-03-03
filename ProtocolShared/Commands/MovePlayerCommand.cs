namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class MovePlayerCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.MovePlayer;
        [Key(0)] public float X { get; set; }
        [Key(1)] public float Y { get; set; }
    }
}