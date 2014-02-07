using LiveFoosball.Dal.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveFoosball.Dal
{
    public enum Team
    {
        Red = 1,
        Blue = 2
    }


    public static class FoosballCollections
    {
        private static ConcurrentDictionary<Guid, object> _gameLocks = new ConcurrentDictionary<Guid, object>();

        private static MongoCollection<GoalEntry> goalEntries;

        static FoosballCollections()
        {
            var connectionString = "mongodb://appharbor_2616a298-6146-425e-a4b6-34cf178d5055:je2eihdhegjh250j6c64377mln@ds027799.mongolab.com:27799/appharbor_2616a298-6146-425e-a4b6-34cf178d5055";
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var db = server.GetDatabase("appharbor_2616a298-6146-425e-a4b6-34cf178d5055");
            goalEntries = db.GetCollection<GoalEntry>("Goals");
        }


        public static List<GoalEntry> GetGoalsForGame(Guid gameId)
        {
            var q = Query<GoalEntry>.EQ(ge => ge.GameId, gameId);
            var goals = goalEntries.Find(q);
            return goals.ToList();
        }

        public static void UpdateGameById(Guid gameId, int red, int blue)
        {
            lock (_lockGame(gameId))
            {

            }
        }

        private static object _lockGame(Guid gameId)
        {
            object gameLock = new object();
            if (!_gameLocks.TryAdd(gameId, gameLock))
            {
                _gameLocks.TryGetValue(gameId, out gameLock);
            }


            return gameLock;
        }

        public static void AddGoalForGame(Guid gameId, Team team, DateTime goalDate)
        {
            var goal = new GoalEntry(gameId, team, goalDate);
            goalEntries.Insert<GoalEntry>(goal);
        }
    }
}
