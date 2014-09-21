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
using System.Configuration;

namespace projectionsConsole
{
    class Program
    {

        

        static void Main(string[] args)
        {
            Console.Write("projection started...");

            var EventStoreHost = ConfigurationManager.AppSettings["EventStoreHost"];
            var EventStoreHttpPort = int.Parse(ConfigurationManager.AppSettings["EventStoreHttpPort"]);
            var EventStoreTCPPort = int.Parse(ConfigurationManager.AppSettings["EventStoreTCPPort"]);

            var tcpEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(EventStoreHost), EventStoreTCPPort);
            var httpendPoint = new IPEndPoint(System.Net.IPAddress.Parse(EventStoreHost), EventStoreHttpPort);

            var user = new UserCredentials("admin", "changeit");
            
            var conn = EventStoreConnection.Create(tcpEndPoint);
            conn.Connect();

            var log = new EventStore.ClientAPI.Common.Log.ConsoleLogger();
            var pm = new EventStore.ClientAPI.ProjectionsManager(log, httpendPoint);

            var proj = System.IO.File.ReadAllText(@"..\..\projections\gameListProjection.js");
            pm.CreateContinuous("gameListProjection", proj, user);

            var subscription = new Subscription(conn,true, user);
            var b2bConnstring = ConfigurationManager.ConnectionStrings["bear2bear"];
            using (var sqliteConnection = new SQLiteConnection(b2bConnstring.ConnectionString))
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
