using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace LiveFoosball.SignalR
{
    public enum Team
    {
        Blue = 1,
        Red = 2
    }

    public class FoosballHub : Hub
    {
        public void Goal(Team team)
        {
            Clients.All.goal(team);
        }

        public void StartGame()
        {
            Clients.All.canStartGame();
        }
    }
}