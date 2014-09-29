using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glowing.core
{
    public interface ISqliteConnection : IDisposable
    {
        void Open();
        ISqliteCommand CreateCommand(string sql);
    }

    public interface ISqliteCommand
    {
        void Add(string name, string value);
        object ExecuteScalar();
        int ExecuteNonQuery();
    }

}
