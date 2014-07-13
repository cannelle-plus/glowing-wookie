using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using System.Net;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using System.Data.SQLite;

namespace projectionsConsole
{
    class Program
    {

        public static IEnumerable<GameEvent> retrieveAllEvents (string gameId)
        {
            var user = new UserCredentials("admin", "changeit");
            var endPoint = new IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 1113);
            var conn = EventStoreConnection.Create(endPoint);
            conn.Connect();
            var slice = conn.ReadStreamEventsForward("Game-81b4e35a8fa8499096c457489c750b8b", 0, Int32.MaxValue, false, user);

            var evts = new List<GameEvent>();

            foreach(var e in slice.Events)
            {
                var json = System.Text.Encoding.UTF8.GetString(e.Event.Data);
                var evt = JsonConvert.DeserializeObject<JsonEvent>(json);
                
                switch (evt.@case)
                {
                    case "GameCreated" :
                        evts.Add(new GameCreated()
                        {
                            GameId = Guid.Parse(evt.value[0]),
                            UserName = evt.value[1],
                            CreationDate = DateTime.Parse(evt.value[2]),
                            GameDate = DateTime.Parse(evt.value[3]),
                            GameLocation = evt.value[4]
                        });
                        break;
                    case "GameJoined":
                        evts.Add(new GameJoined()
                        {
                            GameId = Guid.Parse(evt.value[0]),
                            UserName = evt.value[1]
                        });
                        break;
                    case "GameAbandonned":
                        evts.Add(new GameJoined()
                        {
                            GameId = Guid.Parse(evt.value[0]),
                            UserName = evt.value[1]
                        });
                        break;
                    default:
                        break;
                }
            }
            return evts;
            
        }

        static void Main(string[] args)
        {
            Console.Write("projection started...");
            using (var sqliteConnection = new SQLiteConnection(@"Data Source=D:\Projects\db-wookie\db\drawTeams.db;Version=3"))
            { 
                //setting up projections
                sqliteConnection.Open();
                var gamesListProjection = new GamesListProjection(sqliteConnection);

                //lsit of all projections availables
                var projections = new Dictionary<Type,Action<Event>>();
                
                //wiring up event to dedicated projections
                projections.Add(typeof(GameCreated), e => gamesListProjection.Handle((GameCreated)e));
                projections.Add(typeof(GameJoined), e => gamesListProjection.Handle((GameJoined)e));
                projections.Add(typeof(GameAbandonned), e => gamesListProjection.Handle((GameAbandonned)e));

                foreach (var evt in retrieveAllEvents("Game-81b4e35a8fa8499096c457489c750b8b"))
                {
                    projections[evt.GetType()](evt);
                }
            }

            Console.Write("projection finished...");
            Console.Read();
            
        }
        
    }
}
