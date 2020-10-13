using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AveneoTask.Database
{
    public class MySQLDB
    {
        public MySqlConnection Connection { get; }

        public MySQLDB(string connectionString)
        {
            Connection = new MySqlConnection(connectionString);
        }

        public void Dispose() => Connection.Dispose();
    }
}
