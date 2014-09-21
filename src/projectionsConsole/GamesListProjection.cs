using EventStore.ClientAPI;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace projectionsConsole
{
    public static class sqliteExtension
    {
        public static void Add(this SQLiteParameterCollection collection, string name, string value)
        {
            collection.Add(new SQLiteParameter(name));
            collection[name].Value = value;
        }
    }

    public interface Projection
    {
        string Name {get;}
        void Handle(ResolvedEvent e);
        void OnError(Exception e);
    }

    public class GamesListProjection :Projection
    {
            //fromAll() 
            //.when({
            //    GameCreated : function(s, e) {
            //        linkTo('Proj-GamesList', e);
            //        return s;
            //    }
            //})

        private SQLiteConnection _connection;
        private string _name = "Proj-GamesList";

        public string Name 
        { 
            get { return _name; } 
        }

        public GamesListProjection( SQLiteConnection connection)
        {
            _connection = connection;
            
        }

        public void Handle(ResolvedEvent e)
        {
            var jsonMeta = System.Text.Encoding.UTF8.GetString(e.Event.Metadata);
            var meta = JsonConvert.DeserializeObject<MetaData>(jsonMeta);

            var json = System.Text.Encoding.UTF8.GetString(e.Event.Data);
            var evt = JsonConvert.DeserializeObject<JsonEvent>(json);

            switch (evt.@case)
            {
                case "GameCreated":
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
                default:
                    break;
            }
        }
        
        
        public void OnError(Exception e)
        {
            Console.WriteLine(e);
        }
        

        private void Handle(GameCreated evt)
        {
            
            var sql = "Insert into GamesList VALUES (@id,0, @name,@ownerId,@ownerUserName, @begins, @location, @players,@nbPlayers, @maxPlayers);";
            var cmd = new SQLiteCommand(sql,_connection);
            cmd.Parameters.Add("@id", evt.AggregateId.ToString());
            cmd.Parameters.Add("@name", evt.Name);
            cmd.Parameters.Add("@ownerId", evt.OwnerId);
            cmd.Parameters.Add("@ownerUserName", evt.OwnerUserName);
            cmd.Parameters.Add("@begins", evt.Date.ToString());
            cmd.Parameters.Add("@location", evt.Location.ToString());
            cmd.Parameters.Add("@players", evt.Username);
            cmd.Parameters.Add("@nbPlayers", "1");
            cmd.Parameters.Add("@maxPlayers", evt.nbPlayersRequired.ToString());

            cmd.ExecuteNonQuery();
        }

        public void Handle(GameJoined evt)
        {
            var sql = "update GamesList set players = players || \" \"|| @newPlayer where id=@id;";
            var cmd = new SQLiteCommand(sql, _connection);
            cmd.Parameters.Add("@id", evt.AggregateId.ToString());
            cmd.Parameters.Add("@newPlayer", evt.Username.ToString());

            cmd.ExecuteNonQuery();
        }

        public void Handle(GameAbandonned evt)
        {
            var sql = "update GamesList set players = REPLACE(players, @oldPlayer,'') where idd=@d;";
            var cmd = new SQLiteCommand(sql, _connection);
            cmd.Parameters.Add("@id", evt.AggregateId.ToString());
            cmd.Parameters.Add("@oldPlayer", evt.Username.ToString());

            cmd.ExecuteNonQuery();

        }
    }
}
