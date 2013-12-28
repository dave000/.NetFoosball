using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using LiveFoosball.GameData;

namespace LiveFoosball.SignalR
{
    public class FoosballHub : Hub
    {
        public GameInfo StartGame(Guid? gameId)
        {
            var clientId = Context.ConnectionId;

            if (gameId.HasValue)
            {
                var gameInfo = GameStarter.StartPendingGame(gameId.Value);
                return gameInfo;
            }
            else
            {
                var gameInfo = GameStarter.TryStartGame(clientId);
                dynamic client = Clients.Client(Game.Current.Info.ClientId);
                if (!gameInfo.Started && client != null)
                {
                    client.canStartGame(gameInfo);
                }
                
                return gameInfo;
            }



        }

        public void CancelPendingGame(GameInfo gameInfo)
        {
            GameStarter.CancelPendingGame(gameInfo.Id);
            var client = Clients.Client(gameInfo.ClientId);
            if (client != null)
            {
                client.gameCanceled(gameInfo.Id);
            }
            
        }
    }
}