namespace ServerUnity.Rooms
{
    using System.Collections.Concurrent;

    public static class GameRoomManager
    {
        private static readonly ConcurrentDictionary<string, GameRoom> _rooms;

        static GameRoomManager()
        {
            GameRoomManager._rooms = new ConcurrentDictionary<string, GameRoom>();
            GameRoomManager.Create("default");
        }

        /// <summary>
        /// Try to add a new room with an id
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public static bool Create(string roomId)
        {
            return GameRoomManager._rooms.TryAdd(roomId, new GameRoom(roomId));
        }

        /// <summary>
        /// Try to get a room by an id
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        public static bool TryGet(string roomId, out GameRoom room)
        {
            return GameRoomManager._rooms.TryGetValue(roomId, out room);
        }
    }
}