using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using  System.Collections.Concurrent;


namespace LiveFoosball.GameData
{
    public static class GameStarter
    {
        private static ConcurrentDictionary<Guid, GameInfo> _pendingGames = new ConcurrentDictionary<Guid, GameInfo>();
        public static GameInfo TryStartGame(string clientId)
        {
            lock(_pendingGames)
            {
                Guid gameId = Guid.NewGuid();
                GameInfo gameInfo;
                if (Game.Current != null && !Game.Current.Finished)
                {
                    gameInfo = new GameInfo(gameId, clientId);
                    _pendingGames.TryAdd(gameId, gameInfo);
                }
                else
                {
                   gameInfo = Game.StartNewGame(gameId, clientId);
                }
                
                return gameInfo;
            }
            
        }

        public static GameInfo StartPendingGame(Guid gameId)
        {
            GameInfo info;
            _pendingGames.TryRemove(gameId, out info);
            if (info != null)
            {
                return Game.StartNewGame(info.Id, info.ClientId);
            }

            return null;

        }

        internal static void CancelPendingGame(Guid guid)
        {
            GameInfo info;
            _pendingGames.TryRemove(guid, out info);
        }
    }
}