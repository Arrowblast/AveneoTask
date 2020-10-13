using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AveneoTask.Database.Models
{
    [Table("aveneo.apikeys")]
    public class ApiKeys
    {
        [Key]
        public string AppID { get; set; }
        public string ApiKey { get; set; }

        internal MySQLDB Db { get; set; }

        public ApiKeys()
        {
        }

        internal ApiKeys(MySQLDB db)
        {
            Db = db;
        }
        public async Task InsertAsync()
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO `ApiKeys` (`AppID`, `ApiKey`) VALUES (@appid, @apikey);";
            BindParams(cmd);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync()
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"UPDATE `ApiKeys` SET `ApiKey` = @apikey WHERE `AppID` = @appid;";
            BindKey(cmd);
            BindId(cmd);
            await cmd.ExecuteNonQueryAsync();
        }
        private void BindId(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@appid",
                DbType = DbType.String,
                Value = AppID,
            });
        }
        private void BindKey(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@apikey",
                DbType = DbType.String,
                Value = ApiKey,
            });
        }

        private void BindParams(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@appid",
                DbType = DbType.String,
                Value = AppID,
            });
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@apikey",
                DbType = DbType.String,
                Value = ApiKey,
            });
        }
    }
}
