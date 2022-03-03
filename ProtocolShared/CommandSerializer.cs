namespace ProtocolShared
{
    using System;
    using MessagePack;

    public static class CommandSerializer
    {
        public static byte[] Serialize(ICommand command) => MessagePackSerializer.Serialize(command.GetType(), command);
        
        public static ICommand Deserialize(Type type, byte[] bytes) => (ICommand) MessagePackSerializer.Deserialize(type, bytes);
    }
}