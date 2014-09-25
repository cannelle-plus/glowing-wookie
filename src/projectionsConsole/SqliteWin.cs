using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace projectionsConsole
{

    public class SqliteConnectionWin :ISqliteConnection
    {
        SQLiteConnection _connection;

        public SqliteConnectionWin(string dbConnection)
        {
            _connection = new SQLiteConnection(dbConnection);
        }

        public void Open()
        {
            _connection.Open();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }


        public ISqliteCommand CreateCommand(string sql)
        {
            var cmd = new SQLiteCommand(sql, _connection);
            return new SqliteCommandWin(cmd);
        }
    }

    public class SqliteCommandWin : ISqliteCommand
    {
        SQLiteCommand _cmd;

        public SqliteCommandWin(SQLiteCommand cmd)
        {
            _cmd = cmd;
        }
        
        public void Add(string name, string value)
        {
            _cmd.Parameters.Add(new SQLiteParameter(name));
            _cmd.Parameters[name].Value = value;
        }

        public object ExecuteScalar()
        {
            return _cmd.ExecuteScalar();
        }

        public int ExecuteNonQuery()
        {
            return _cmd.ExecuteNonQuery();
        }
    }
}
