using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using glowing.core;
using System.Net;
using EventStore.ClientAPI.SystemData;
using System.Configuration;

namespace glowing.projections
{

    public class GamesListProjection : Projection
    {
        private ISqliteConnection _connection;
        private string _name = "Proj-GamesList";
        private int? _lastCheckPoint = null;
        private string _username;
        private string _password;
        private IPEndPoint _httpendPoint;
        private bool _isStarted;
        private Action _onError;

        public string Name 
        { 
            get { return _name; } 
        }

        public int? LastCheckPoint
        {
            get { return _lastCheckPoint; }
        }

        public GamesListProjection(IPEndPoint httpendPoint, string username, string password, ISqliteConnection connection, Action onError)
        {
            _connection = connection;
            _username = username;
            _password = password;
            _httpendPoint = httpendPoint;
            _onError = onError;
        }


        public void Start()
        {
            var user = new UserCredentials(_username, _password);

            var log = new EventStore.ClientAPI.Common.Log.ConsoleLogger();
            var pm = new EventStore.ClientAPI.ProjectionsManager(log, _httpendPoint);

            var projectionsPath = ConfigurationManager.AppSettings["projectionsPath"];

            //test if projections is not already created
            //should be amended
            var proj = System.IO.File.ReadAllText(projectionsPath + "gameListProjection.js");
            try
            {
                pm.CreateContinuous("gameListProjection", proj, user);
            }
            catch (Exception e)
            {
                Console.WriteLine("the projection gameListProjection was not created ");
                Console.WriteLine(proj);
            }
           
        }

        public void Handle(ResolvedEvent e)
        {
            
            var sql = "Select count(*) from Projections where name='" + _name + "' and  messageIdProcessed=@eventId;";
            var cmd = _connection.CreateCommand(sql);
            cmd.Add("@eventId", e.OriginalEvent.EventId.ToString());
            var isProcessed = (long)cmd.ExecuteScalar();

            

            if (isProcessed == 0)
            {

                var jsonMeta = System.Text.Encoding.UTF8.GetString( e.Event.Metadata);
                var meta = JsonConvert.DeserializeObject<MetaData>(jsonMeta);

                var json = System.Text.Encoding.UTF8.GetString(e.Event.Data);
                var evt = JsonConvert.DeserializeObject<JsonEvent>(json);

            
                switch (evt.@case)
                {
                    case "GameScheduled":
                        Handle(new GameCreated()
                        {
                            UserId = meta.UserId,
                            Username = meta.UserName,
                            CorrelationId = meta.CorrelationId,
                            EventId = e.Event.EventId,
                            Version = e.Event.EventNumber,
                            AggregateId = e.Event.EventStreamId,
                            Name = evt.value[0],
                            OwnerId = evt.value[1],
                            Date = DateTime.Parse(evt.value[2]),
                            Location = evt.value[3],
                            nbPlayersRequired = int.Parse(evt.value[4]),
                            OwnerUserName = meta.UserName // tweak: I should get it from the event directly like its id
                        });
                        break;
                    case "GameJoined":
                        Handle(new GameJoined()
                        {
                            UserId = meta.UserId,
                            Username = meta.UserName,
                            CorrelationId = meta.CorrelationId,
                            EventId = e.Event.EventId,
                            Version = e.Event.EventNumber,
                            AggregateId = e.Event.EventStreamId
                        });
                        break;
                    case "GameAbandonned":
                        Handle(new GameAbandonned()
                        {
                            UserId = meta.UserId,
                            Username = meta.UserName,
                            CorrelationId = meta.CorrelationId,
                            EventId = e.Event.EventId,
                            Version = e.Event.EventNumber,
                            AggregateId = e.Event.EventStreamId
                        });
                        break;
                    default:
                        break;
                }



                var sqlInsertProj = "Insert into Projections VALUES ('" + _name + "',@eventId);";
                var cmdInsertProj = _connection.CreateCommand(sqlInsertProj);
                cmdInsertProj.Add("@eventId", e.OriginalEvent.EventId.ToString());
                cmdInsertProj.ExecuteNonQuery();
            }

            _lastCheckPoint = e.Event.EventNumber;
        }
        
        
        public void OnError(Exception e)
        {
            Console.WriteLine(e);
            _onError();
        }
        

        private void Handle(GameCreated evt)
        {
                        
            var sql = "Insert into GamesList VALUES (@id,0, @name,@ownerId,@ownerUserName, @begins, @location, @players,@nbPlayers, @maxPlayers);";
            var cmd = _connection.CreateCommand(sql);
            cmd.Add("@id", evt.getAggregateId());
            cmd.Add("@name", evt.Name);
            cmd.Add("@ownerId", evt.OwnerId);
            cmd.Add("@ownerUserName", evt.OwnerUserName);
            cmd.Add("@begins", evt.Date.ToString());
            cmd.Add("@location", evt.Location.ToString());
            cmd.Add("@players", evt.Username);
            cmd.Add("@nbPlayers", "1");
            cmd.Add("@maxPlayers", evt.nbPlayersRequired.ToString());

            cmd.ExecuteNonQuery();
        }

        public void Handle(GameJoined evt)
        {
            var sql = "update GamesList set players = players || \" \"|| @newPlayer ,nbPlayers= nbPlayers+1 where id=@id;";
            var cmd = _connection.CreateCommand(sql);
            cmd.Add("@id", evt.getAggregateId());
            cmd.Add("@newPlayer", evt.Username.ToString());

            cmd.ExecuteNonQuery();
        }

        public void Handle(GameAbandonned evt)
        {
            var sql = "update GamesList set players = REPLACE(players, @oldPlayer,''),nbPlayers= nbPlayers-1 where id=@id;";
            var cmd = _connection.CreateCommand(sql);
            cmd.Add("@id", evt.getAggregateId());
            cmd.Add("@oldPlayer", evt.Username.ToString());

            cmd.ExecuteNonQuery();

        }

        
    }
}
