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

    public class BearListProjection : Projection
    {
        private ISqliteConnection _connection;
        private string _name = "Proj-BearList";
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

        public BearListProjection(IPEndPoint httpendPoint, string username, string password, ISqliteConnection connection, Action onError)
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
            var proj = System.IO.File.ReadAllText(projectionsPath + "bearProjection.js");
            try
            {
                pm.CreateContinuous("bearProjection", proj, user);
            }
            catch (Exception e)
            {
                Console.WriteLine("the projection bearProjection was not created ");
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
                    case "SignedIn":
                        Handle(new SignedIn()
                        {
                            UserId = meta.UserId,
                            Username = meta.UserName,
                            CorrelationId = meta.CorrelationId,
                            EventId = e.Event.EventId,
                            Version = e.Event.EventNumber,
                            AggregateId = e.Event.EventStreamId,
                            bearName = evt.value[0],
                            bearSocialId = evt.value[1],
                            bearAvatarId = evt.value[2]
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
        

        private void Handle(SignedIn evt)
        {

            var sql = "Insert into bears VALUES (@id, @username,@avatarId); Insert into Users VALUES (@bearSocialId, @id);";
            var cmd = _connection.CreateCommand(sql);
            cmd.Add("@id", evt.getAggregateId());
            cmd.Add("@username", evt.bearName);
            cmd.Add("@avatarId", evt.bearAvatarId);
            cmd.Add("@bearSocialId", evt.bearSocialId);

            cmd.ExecuteNonQuery();
        }

        

        
    }
}
