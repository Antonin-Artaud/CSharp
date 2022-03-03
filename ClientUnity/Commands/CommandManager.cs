namespace ClientUnity.Commands;

using System.Text.Json;
using ProtocolShared;
using ProtocolShared.Commands;
using ProtocolShared.Commands.Entity;

public class CommandManager
{
    public ClientUnity ServerConnection { get; }

    public CommandManager(ClientUnity serverConnection)
    {
        this.ServerConnection = serverConnection;
    }
        
    public void HandleCommand(ICommand command)
    {
        Console.WriteLine("[CommandManager] {0} {1}", command.Id, JsonSerializer.Serialize(command, command.GetType()));

        switch (command.Id)
        {
            case CommandId.AuthenticationSuccess:
                this.OnAuthenticationSuccess((AuthenticationSuccessCommand)command);
                break;
            case CommandId.CreateRoomResponse:
                this.OnCreateRoomResponse((CreateRoomResponseCommand) command);
                break;
            case CommandId.JoinRoomResponse:
                this.OnJoinRoomResponse((JoinRoomResponseCommand) command);
                break;
            case CommandId.EntityData:
                this.OnEntityData((EntityDataCommand) command);
                break;
            case CommandId.EntityRemoved:
                this.OnEntityRemoved((EntityRemovedCommand) command);
                break;
            case CommandId.None:
                break;
            case CommandId.KeepAliveRequest:
                break;
            case CommandId.KeepAliveResponse:
                break;
            case CommandId.Authentication:
                break;
            case CommandId.AuthenticationError:
                break;
            case CommandId.CreateRoomRequest:
                break;
            case CommandId.JoinRoomRequest:
                break;
            case CommandId.MovePlayer:
                break;
            case CommandId.KillPlayer:
                break;
            case CommandId.OpenDoor:
                break;
            default:
                Console.WriteLine("[CommandManager] Unknown command {0}", command.Id);
                break;
        }
    }

    private void OnAuthenticationSuccess(AuthenticationSuccessCommand command)
    {
        this.SendCommand(new CreateRoomRequestCommand
        {
            RoomId = "BIG HAPPY"
        });
    }

    private void OnCreateRoomResponse(CreateRoomResponseCommand command)
    {
        if (command.Success)
        {
            this.SendCommand(new JoinRoomRequestCommand
            {
                RoomId = "BIG HAPPY"
            });
        }
    }

    private void OnJoinRoomResponse(JoinRoomResponseCommand command)
    {
        if (command.Success)
        {
            this.SendCommand(new JoinRoomResponseCommand()
            {
                Success = true,
            });
        }
    }
    
    private void OnEntityData(EntityDataCommand command)
    {
        GameObject entityObject = MonoNetworkEntity.Find(command.Entity.Id);

        if (entityObject != null)
            entityObject.GetComponent<MonoNetworkEntity>().OnEntityDataReceived(command.Entity);
        else
            MonoNetworkEntity.Create(command.Entity);
    }

    private void OnEntityRemoved(EntityRemovedCommand command)
    {
        GameObject entityObject = MonoNetworkEntity.Find(command.EntityId);

        if (entityObject != null)
        {
            Object.Destroy(entityObject);
        }
    }

    private void SendCommand(ICommand command)
    {
        this.ServerConnection.Connection.Send(command);
    }
}
