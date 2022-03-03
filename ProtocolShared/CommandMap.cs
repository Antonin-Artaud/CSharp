namespace ProtocolShared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CommandMap
    {
        private static readonly Dictionary<Type, CommandId> CommandIdByType;
        private static readonly Dictionary<CommandId, Type> CommandTypeById;

        static CommandMap()
        {
            CommandMap.CommandIdByType = new Dictionary<Type, CommandId>();
            CommandMap.CommandTypeById = new Dictionary<CommandId, Type>();

            foreach (Type type in typeof(ICommand).Assembly.GetTypes().Where(t => t.IsClass && typeof(ICommand).IsAssignableFrom(t)))
            {
                try
                {
                    ICommand command = (ICommand) Activator.CreateInstance(type);
                    CommandId commandId = command.Id;

                    if (commandId != CommandId.None)
                    {
                        CommandMap.CommandIdByType.Add(type, commandId);
                        CommandMap.CommandTypeById.Add(commandId, type);   
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to create a new instance of command type " + type.Name);
                }
            }
        }

        public static Type GetCommandTypeById(CommandId id) => CommandMap.CommandTypeById.TryGetValue(id, out Type type) ? type : null;
        public static CommandId GetCommandIdByType(Type type) => CommandMap.CommandIdByType.TryGetValue(type, out CommandId id) ? id : CommandId.None;
    }
}