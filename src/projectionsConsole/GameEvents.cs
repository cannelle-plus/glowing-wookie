using EventStore.ClientAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projectionsConsole
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

}
