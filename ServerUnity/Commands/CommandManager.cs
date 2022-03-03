namespace ServerUnity.Commands
{
    using System;
    using System.Text.Json;
    using ProtocolShared;
    using ProtocolShared.Commands;
    using ServerUnity.Network;
    using ServerUnity.Rooms;
    using ServerUnity.Session;

    public class CommandManager
    {
        public ClientConnection ClientConnection { get; }

        public CommandManager(ClientConnection clientConnection)
        {
            this.ClientConnection = clientConnection;
        }
        
        /// <summary>
        /// receive command data, send this command
        /// </summary>
        /// <param name="command"></param>
        public void HandleCommand(ICommand command)
        {
            Console.WriteLine("[RCV] {0} {1}", command.Id, JsonSerializer.Serialize(command, command.GetType()));

            switch (command.Id)
            {
                case CommandId.Authentication:
                    this.OnAuthentication((AuthenticationCommand) command);
                    break;
                case CommandId.CreateRoomRequest:
                    this.OnCreateRoomRequest((CreateRoomRequestCommand) command);
                    break;
                case CommandId.JoinRoomRequest:
                    this.OnJoinRoomRequest((JoinRoomRequestCommand) command);
                    break;
                case CommandId.KillPlayer:
                    this.OnKillPlayer((KillPlayerCommand) command);
                    break;
                case CommandId.OpenDoor:
                    this.OnOpenDoor((OpenDoorCommand) command);
                    break;
                case CommandId.MovePlayer:
                    this.OnMovePlayer((MovePlayerCommand) command);
                    break;
            }
        }

        /// <summary>
        /// Create new ClientSession and send successAuthenticationCommand
        /// </summary>
        /// <param name="command"></param>
        private void OnAuthentication(AuthenticationCommand command)
        {
            this.ClientConnection.Session = new ClientSession(command.UserId, this.ClientConnection);
            this.SendCommand(new AuthenticationSuccessCommand());
        }

        /// <summary>
        /// Create new room
        /// </summary>
        /// <param name="command"></param>
        private void OnCreateRoomRequest(CreateRoomRequestCommand command)
        {
            this.SendCommand(new CreateRoomResponseCommand
            {
                Success = GameRoomManager.Create(command.RoomId)
            });
        }
        
        /// <summary>
        /// try to join a room
        /// </summary>
        /// <param name="command"></param>
        private void OnJoinRoomRequest(JoinRoomRequestCommand command)
        {
            if (this.ClientConnection.Session.CurrentRoom != null)
            {
                this.ClientConnection.Session.CurrentRoom.Leave(this.ClientConnection.Session.UserId);
                this.ClientConnection.Session.CurrentRoom = null;
            }
            
            if (GameRoomManager.TryGet(command.RoomId, out GameRoom room) && room.Join(this.ClientConnection.Session.UserId, this.ClientConnection))
            {
                this.ClientConnection.Session.CurrentRoom = room;
                this.SendCommand(new JoinRoomResponseCommand
                {
                    Success = true
                });
            }
            else
            {
                this.SendCommand(new JoinRoomResponseCommand
                {
                    Success = false
                });
            }
        }
        
        /// <summary>
        /// Call function OnKillPlayer of GameRoom
        /// </summary>
        /// <param name="command"></param>
        private void OnKillPlayer(KillPlayerCommand command)
        {
            GameRoom currentRoom = this.ClientConnection.Session.CurrentRoom;

            currentRoom?.OnKillPlayer(this.ClientConnection.Session.UserId, command.EntityId);
        }
        
        /// <summary>
        /// Call function OnMovePlayer of GameRoom
        /// </summary>
        /// <param name="command"></param>
        private void OnMovePlayer(MovePlayerCommand command)
        {
            GameRoom currentRoom = this.ClientConnection.Session.CurrentRoom;

            currentRoom?.OnMovePlayer(this.ClientConnection.Session.UserId, command.X, command.Y);
        }

        /// <summary>
        /// Call function OnOpenDoor of GameRoom
        /// </summary>
        /// <param name="command"></param>
        private void OnOpenDoor(OpenDoorCommand command)
        {
            GameRoom currentRoom = this.ClientConnection.Session.CurrentRoom;

            currentRoom?.OnOpenDoor(command.DoorId);
        }

        /// <summary>
        /// Send Command
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(ICommand command)
        {
            Console.WriteLine("[SND] {0} {1}", command.GetType().Name, JsonSerializer.Serialize(command, command.GetType()));
            this.ClientConnection.Connection.Send(command);
        }
    }
}