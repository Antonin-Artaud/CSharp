namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class AuthenticationSuccessCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.AuthenticationSuccess;
    }
}