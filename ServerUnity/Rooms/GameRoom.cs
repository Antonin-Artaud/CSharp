namespace ServerUnity.Rooms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ProtocolShared.Commands.Entity;
    using ServerUnity.Network;

    public class GameRoom
    {
        public string Id { get; }
        public List<PlayerInfo> Players { get; }
        public List<GameEntity> Entities { get; }

        private uint _entityCounter;
        public GameRoom(string id)
        {
            this.Id = id;
            this.Players = new List<PlayerInfo>();
            this.Entities = new List<GameEntity>();
        }
        
        /// <summary>
        /// Get the entity of the player to be killed by id (killerUserId),
        /// and the target (targetEntityId) which will have its status set to "dead".
        /// </summary>
        /// <param name="killerUserId"></param>
        /// <param name="targetEntityId"></param>
        public void OnKillPlayer(uint killerUserId, uint targetEntityId)
        {
            PlayerInfo playerInfo = this.GetPlayer(killerUserId);

            if (playerInfo == null || playerInfo.PlayerEntity.State != PlayerState.Killer)
                return;

            PlayerEntity targetEntity = this.GetEntityById<PlayerEntity>(targetEntityId);
            
            if (targetEntity == null || targetEntity.State != PlayerState.Self)
                return;

            targetEntity.State = PlayerState.Dead;
            this.OnEntityChanged(targetEntity);
        }
        
        /// <summary>
        /// Get The entity of the player by id and set coordinate
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void OnMovePlayer(uint userId, float x, float y)
        {
            PlayerInfo playerInfo = this.GetPlayer(userId);

            if (playerInfo != null && playerInfo.PlayerEntity.State != PlayerState.Dead)
            {
                playerInfo.PlayerEntity.X = x;
                playerInfo.PlayerEntity.Y = y;
                
                this.OnEntityChanged(playerInfo.PlayerEntity);
            }
        }
        
        /// <summary>
        /// Get door by id and open it 
        /// </summary>
        /// <param name="doorId"></param>
        public void OnOpenDoor(uint doorId)
        {
            DoorEntity doorEntity = (DoorEntity) this.Entities.Find(entity => entity is DoorEntity b && b.DoorId == doorId);

            if (doorEntity != null)
            {
                if (doorEntity.IsOpen)
                    return;

                doorEntity.IsOpen = true;
            }
            else
            {
                doorEntity = new DoorEntity
                {
                    Id = this._entityCounter++,
                    DoorId = doorId,
                    IsOpen = true
                };
                this.Entities.Add(doorEntity);
                
                this.OnEntityChanged(doorEntity);
            }
        }
        
        /// <summary>
        /// Send new update of entities for all player
        /// </summary>
        /// <param name="entity"></param>
        private void OnEntityChanged(GameEntity entity)
        {
            foreach (PlayerInfo playerInfo in this.Players)
            {
                playerInfo.ClientConnection.CommandManager.SendCommand(new EntityDataCommand
                {
                    Entity = entity
                });
            }
        }
        
        /// <summary>
        /// Remove a specific entity by id
        /// </summary>
        /// <param name="entityId"></param>
        private void OnEntityRemoved(uint entityId)
        {
            foreach (PlayerInfo playerInfo in this.Players)
            {
                playerInfo.ClientConnection.CommandManager.SendCommand(new EntityRemovedCommand
                {
                    EntityId = entityId
                });
            }
        }
        
        /// <summary>
        /// Get entity by id in list of GameEntity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("GameEntity GetEntityById(uint id) is deprecated, please use GetEntityById<T> instead")]
        private GameEntity GetEntityById(uint id) => this.Entities.Find(_ => _.Id == id);
        
        /// <summary>
        /// return a specific type of GameEntity by an id
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetEntityById<T>(uint id) where T : GameEntity => this.Entities.Find(entity => entity.Id == id) as T;
        
        /// <summary>
        /// Get playerInfo by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private PlayerInfo GetPlayer(uint userId) => this.Players.Find(p => p.UserId == userId);
        
        /// <summary>
        /// get index of specific player
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private int GetPlayerIndex(uint userId) => this.Players.FindIndex(p => p.UserId == userId);
        
        /// <summary>
        /// try to create and add a new player
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="clientConnection"></param>
        /// <returns></returns>
        public bool Join(uint userId, ClientConnection clientConnection)
        {
            if (this.GetPlayerIndex(userId) == -1)
            {
                PlayerEntity playerEntity = new PlayerEntity
                {
                    Id = this._entityCounter++,
                    OwnerId = userId,
                    State = Enumerable.Any(this.Entities, entity => entity is PlayerEntity {State: PlayerState.Killer}) ? PlayerState.Self : PlayerState.Killer,
                    X = 0,
                    Y = 0
                };
                this.Entities.Add(playerEntity);
                this.Players.Add(new PlayerInfo(userId, clientConnection, playerEntity));

                foreach (PlayerInfo playerInfo in this.Players)
                {
                    if (playerInfo.UserId != userId)
                    {
                        playerInfo.ClientConnection.CommandManager.SendCommand(new EntityDataCommand
                        {
                            Entity = playerEntity
                        });
                    }
                }
                
                foreach (var gameEntity in this.Entities)
                {
                    clientConnection.CommandManager.SendCommand(new EntityDataCommand
                    {
                        Entity = gameEntity
                    });
                }
                
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// remove playerEntity
        /// </summary>
        /// <param name="userId"></param>
        public void Leave(uint userId)
        {
            int playerIndex = this.GetPlayerIndex(userId);

            if (playerIndex != -1)
            {
                var playerEntity = this.Players[playerIndex].PlayerEntity;
                this.Players.RemoveAt(playerIndex);
                this.Entities.Remove(playerEntity);
                this.OnEntityRemoved(playerEntity.Id);
            }
        }
    }
}