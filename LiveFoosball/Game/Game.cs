using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiveFoosball.Game
{
    public class Score
    {
        public byte Red { get; set; }
        public byte Blue { get; set; }
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
        public GameInfo Info { get; private set; }



        public Score Score { get; private set; }

        public bool Finished
        {
            get
            {
                return Score.Blue == 10 || Score.Red == 10;
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
    }
}