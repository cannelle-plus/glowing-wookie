using EventStore.ClientAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glowing.projections
{
    public class JsonEvent
    {
        public string @case { get; set; }
        public string[] value { get; set; }    
    }

    public abstract class Event
    {
        public Guid EventId { get; set; }
        public int Version { get; set; }
        public string AggregateId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public Guid CorrelationId { get; set; }

        public string getAggregateId()
        {
            var indexGuid = AggregateId.IndexOf("-");
            if (indexGuid > -1 && AggregateId.Length > 1)
                return AggregateId.Substring(indexGuid + 1);
            else
                return AggregateId;
        }
    }

    public class GameCreated : Event
    {
        public string OwnerId { get; set; }
        public string OwnerUserName { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public int nbPlayersRequired { get; set; }
        

        
    }

    public class GameJoined :Event {}

    public class GameAbandonned : Event { }

    public class SignedIn : Event {
        public string bearName { get; set; }
        public string bearAvatarId { get; set; }
        
        
    }

}
