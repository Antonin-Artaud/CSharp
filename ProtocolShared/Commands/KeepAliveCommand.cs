namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class KeepAliveCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.KeepAliveRequest;
    }
}