namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class KeepAliveServerCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.KeepAliveResponse;
    }
}