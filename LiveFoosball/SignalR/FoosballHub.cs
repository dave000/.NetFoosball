using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace LiveFoosball.SignalR
{
     public class FoosballHub : Hub
    {
        public void StartGame()
        {
            Clients.AllExcept(Context.ConnectionId).canStartGame();
        }
    }
}