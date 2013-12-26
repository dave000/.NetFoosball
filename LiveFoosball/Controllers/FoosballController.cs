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

    public class FoosballController : ApiController
    {
        [ActionName("start"), HttpGet]
        public void StartGame()
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<FoosballHub>();
            hubContext.Clients.All.canStartGame();
        }

        [ActionName("goal"), HttpGet]
        public void Goal()
        {

        }
    }
}