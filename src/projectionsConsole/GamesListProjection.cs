using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projectionsConsole
{
    public interface Projection
    {
        void Handle<T>(T evt) where T: Event;
    }

    public class GamesListProjection
    {
        private SQLiteConnection _connection;

        public GamesListProjection(SQLiteConnection connection)
        {
            _connection = connection;
        }

        public void Handle(GameCreated evt)
        {
            
            var sql = "Insert into GamesList VALUES (@gameId, @gameDate, @gameLocation, @players);";
            var cmd = new SQLiteCommand(sql,_connection);
            cmd.Parameters.Add(new SQLiteParameter("@gameId"));
            cmd.Parameters.Add(new SQLiteParameter("@gameDate"));
            cmd.Parameters.Add(new SQLiteParameter("@gameLocation"));
            cmd.Parameters.Add(new SQLiteParameter("@players"));

            cmd.Parameters["@gameId"].Value = evt.GameId.ToString();
            cmd.Parameters["@gameDate"].Value = evt.GameDate.ToLongDateString();
            cmd.Parameters["@gameLocation"].Value = evt.GameLocation;
            cmd.Parameters["@players"].Value = "";

            cmd.ExecuteNonQuery();
        }

        public void Handle(GameJoined evt)
        {

            var sql = "update GamesList set players = players || \" \"|| @newPlayer where gameId=@gameId;";
            var cmd = new SQLiteCommand(sql, _connection);
            cmd.Parameters.Add(new SQLiteParameter("@gameId"));
            cmd.Parameters.Add(new SQLiteParameter("@newPlayer"));

            cmd.Parameters["@gameId"].Value = evt.GameId.ToString();
            cmd.Parameters["@newPlayer"].Value = evt.UserName;

            cmd.ExecuteNonQuery();
        }

        public void Handle(GameAbandonned evt)
        {
            var sql = "update GamesList set players = REPLACE(players, @oldPlayer,'') where gameId=@gameId;";
            var cmd = new SQLiteCommand(sql, _connection);
            cmd.Parameters.Add(new SQLiteParameter("@gameId"));
            cmd.Parameters.Add(new SQLiteParameter("@oldPlayer"));

            cmd.Parameters["@gameId"].Value = evt.GameId.ToString();
            cmd.Parameters["@oldPlayer"].Value = evt.UserName;

            cmd.ExecuteNonQuery();
        }
    }
}
