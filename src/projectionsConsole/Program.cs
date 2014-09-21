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

        

        static void Main(string[] args)
        {
            Console.Write("projection started...");

            var user = new UserCredentials("admin", "changeit");
            var endPoint = new IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 1113);
            var conn = EventStoreConnection.Create(endPoint);
            conn.Connect();

            var subscription = new Subscription(conn,true, user);

            using (var sqliteConnection = new SQLiteConnection(@"Data Source=D:\Projects\db-wookie\db\Bear2Bear.db;Version=3"))
            { 
                //setting up projections
                sqliteConnection.Open();

                //lsit of all projections availables
                var projections = new List<Projection>();
                
                
                //wiring up event to dedicated projections
                projections.Add(new GamesListProjection(sqliteConnection));

                projections.ForEach((p) => subscription.subscribeTo(p));

                Console.Write("projection working...");
                Console.Read();
            }

            Console.Write("projection finished...");
            
            
        }

    }
}
