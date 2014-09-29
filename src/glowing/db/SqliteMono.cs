using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using glowing.core;

namespace glowing.db
{
    class SqliteConnectionMono : ISqliteConnection
    {
        SqliteConnection _connection;

        public SqliteConnectionMono(string dbConnection)
        {
            _connection = new SqliteConnection(dbConnection);
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
            var cmd = new SqliteCommand(sql, _connection);
            return new SqliteCommandMono(cmd);
        }
    }

    public class SqliteCommandMono : ISqliteCommand
    {
        SqliteCommand _cmd;

        public SqliteCommandMono(SqliteCommand cmd)
        {
            _cmd = cmd;
        }
        
        public void Add(string name, string value)
        {
            _cmd.Parameters.Add(new SqliteParameter(name));
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

