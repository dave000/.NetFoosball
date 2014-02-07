using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveFoosball.Dal.Entities
{
    [BsonIgnoreExtraElements]
    public class GoalEntry
    {
        
        public Team Team { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Binary)]
        public Guid GameId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime TimeStamp { get; set; }

        [BsonConstructor]
        public GoalEntry(Guid gameId, Team team, DateTime timeStamp)
        {
            GameId = gameId;
            Team = team;
            TimeStamp = timeStamp;
            
        }
    }
}
