namespace ProtocolShared.Commands
{
    using MessagePack;

    [MessagePackObject]
    public class AuthenticationCommand : ICommand
    {
        [IgnoreMember] public CommandId Id => CommandId.Authentication;
        [Key(0)] public uint UserId { get; set; }
    }
}