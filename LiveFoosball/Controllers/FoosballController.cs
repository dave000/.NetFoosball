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
    public enum Team
    {
        Blue = 1,
        Red = 2
    }

    [RoutePrefix("api/foosball")]
    public class FoosballController : ApiController
    {
        [Route("start"), HttpGet]
        public void StartGame()
        {
            var hubContext = _getHubContext();
            hubContext.Clients.All.canStartGame();
        }

        [Route("goal/{team}"), HttpGet]
        public Team Goal(Team team)
        {
            var hubContext = _getHubContext();
            hubContext.Clients.All.goal(team);

            return team;
        }

        private IHubContext _getHubContext()
        {
            return GlobalHost.ConnectionManager.GetHubContext<FoosballHub>();
        }
    }
}