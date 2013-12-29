using LiveFoosball.GameData;
using LiveFoosball.SignalR;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LiveFoosball.Controllers
{
    [RoutePrefix("api/foosball")]
    public class FoosballController : ApiController
    {
        [Route("xively")]
        public bool GetXivelyStatus()
        {
            return XivelyClient.Listener.ConnectedToXively;
        }

        [Route("start"), HttpGet]
        public void StartGame()
        {
            var hubContext = _getHubContext();
            hubContext.Clients.All.canStartGame();
        }

        [Route("goal/{team}"), HttpPost]
        public bool Goal(Team team)
        {
            var currentGame = Game.Current;
            if (currentGame != null)
            {
                var hubContext = _getHubContext();
                currentGame.TrackGoal(team, DateTime.UtcNow);
                hubContext.Clients.All.goal(currentGame.Score);
                return true;
            }
            

            return false;
        }

        private IHubContext _getHubContext()
        {
            return GlobalHost.ConnectionManager.GetHubContext<FoosballHub>();
        }
    }
}