using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using glowing.db;
using glowing.projections;
using glowing.core;
using System.Configuration;
using System.Threading;

namespace glowing
{
    class Program
    {

        static bool isMonoRuntime = Type.GetType("Mono.Runtime") != null;
        static bool shutDown = false;
        static EventWaitHandle _waitHandle = new AutoResetEvent(false);
        

        static ISqliteConnection CreateConnection(string dbConnection)
        {
            if (isMonoRuntime)
                return new SqliteConnectionMono(dbConnection);
            else
                return new SqliteConnectionWin(dbConnection);
        }

        static void Main(string[] args)
        {
            Console.Write("projection started...");
            
            var EventStoreHost = ConfigurationManager.AppSettings["EventStoreHost"];
            var EventStoreHttpPort = int.Parse(ConfigurationManager.AppSettings["EventStoreHttpPort"]);
            var EventStoreTCPPort = int.Parse(ConfigurationManager.AppSettings["EventStoreTCPPort"]);

            var tcpEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(EventStoreHost), EventStoreTCPPort);
            var httpendPoint = new IPEndPoint(System.Net.IPAddress.Parse(EventStoreHost), EventStoreHttpPort);

            var username = "admin";
            var password = "changeit";

            var subscription = new Subscription(tcpEndPoint, username, password, true, onError);

            var b2bConnstring = ConfigurationManager.ConnectionStrings["bear2bear"];
            var sqliteConnection = CreateConnection(b2bConnstring.ConnectionString);
        
            //setting up projections
            sqliteConnection.Open();

            //lsit of all projections availables
            var projections = new List<Projection>();

            //wiring up event to dedicated projections
            projections.Add(new GamesListProjection(httpendPoint, username, password, sqliteConnection, onError));
            projections.Add(new BearListProjection(httpendPoint, username, password, sqliteConnection, onError));

            projections.ForEach((p) => subscription.subscribeTo(p));

            _waitHandle.WaitOne();

            subscription.Dispose();
            sqliteConnection.Dispose();

            Console.Write("projection finished...");
            
        }

        public static void onError()
        {
            _waitHandle.Set();
        }

    }
}
