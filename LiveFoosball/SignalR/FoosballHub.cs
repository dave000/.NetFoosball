using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using LiveFoosball.Game;

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

                if (!gameInfo.Started)
                {
                    Clients.Client(Game.Game.Current.Info.ClientId).canStartGame(gameInfo);
                }


                return gameInfo;
            }



        }

        public void CancelPendingGame(GameInfo gameInfo)
        {
            GameStarter.CancelPendingGame(gameInfo.Id);
            Clients.Client(gameInfo.ClientId).gameCanceled(gameInfo.Id);
        }
    }
}