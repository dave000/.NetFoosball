using LiveFoosball.SignalR;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace LiveFoosball.GameData
{
    public enum Team
    {
        Blue = 1,
        Red = 2
    }

    public class Score
    {
        public byte Red { get; set; }
        public byte Blue { get; set; }

        public DateTime LastScore { get; set; }
    }

    public class GameInfo
    {
        public Guid Id { get; private set; }
        public String ClientId { get; private set; }

        public bool Started { get; set; }

        public GameInfo(Guid id, String clientId)
        {
            Id = id;
            ClientId = clientId;
            Started = false;
        }

    }

    public class Game
    {
        public const byte WinScore = 10; 

        public GameInfo Info { get; private set; }

        public Score Score { get; private set; }

        public bool Finished
        {
            get
            {
                return Score.Blue == WinScore || Score.Red == WinScore;
            }
        }

        public static Game Current { get; private set; }

        private Game(Guid id, String clientId)
        {
            Info = new GameInfo(id, clientId);
            Score = new Score();
        }

        public static GameInfo StartNewGame(Guid id, String clientId)
        {
            var newGame = new Game(id, clientId);
            newGame.Info.Started = true;
            Current = newGame;
            return newGame.Info;
        }

        public void TrackGoal(Team team, DateTime goalTime)
        {
            Trace.WriteLine(String.Format("Goal from {0} @ {1}", team, goalTime));
            if (Finished)
            {
                return;
            }

            if (team == Team.Blue)
            {
                Score.Blue++;
            }
            else
            {
                Score.Red++;
            }

            Score.LastScore = goalTime;
        }

        public static void TrackGoal(dynamic goalData)
        {
            if (Current != null && !Current.Finished && goalData != null && goalData.id == "Goal")
            {
                try
                {
                    Team team = Enum.Parse(typeof(Team), goalData.current_value.Value);
                    Current.TrackGoal(team, goalData.at.Value);
                    var hubContext = _getHubContext();
                    hubContext.Clients.All.goal(Current.Score);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error in track goal: " + ex.Message);
                }
                
            }
        }

        private static IHubContext _getHubContext()
        {
            return GlobalHost.ConnectionManager.GetHubContext<FoosballHub>();
        }
    }
}