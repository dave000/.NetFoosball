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
    public class FoosballController : ApiController
    {
        [ActionName("start"), HttpGet]
        public void StartGame()
        {
            GlobalHost.ConnectionManager.GetHubContext<FoosballHub>().Clients.All.canStartGame();
        }
    }
}