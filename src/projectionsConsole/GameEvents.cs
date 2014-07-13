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

    public interface Event
    {
    }

    public interface GameEvent : Event
    {
    }

    public class GameCreated : GameEvent
    {
        public Guid GameId { get; set; }
        public string UserName { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime GameDate { get; set; }
        public string GameLocation { get; set; }
    }

    public class GameJoined : GameEvent
    {
        public Guid GameId { get; set; }
        public string UserName { get; set; }
    }

    public class GameAbandonned : GameEvent
    {
        public Guid GameId { get; set; }
        public string UserName { get; set; }
    }

}
