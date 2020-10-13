using AveneoTask.Database.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace AveneoTask.Database.Query
{
    public class ApiKeysQuery
    {
        public MySQLDB Db { get; }

        public ApiKeysQuery(MySQLDB db)
        {
            Db = db;
        }
        public async Task<ApiKeys> FindOneByKeyAsync(string apikey)
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"SELECT `AppID`, `ApiKey` FROM `ApiKeys` WHERE `ApiKey` = @appkey";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@appkey",
                DbType = DbType.String,
                Value = apikey,
            });
            var result = await ReadAllAsync(await cmd.ExecuteReaderAsync());
            return result.Count > 0 ? result[0] : null;
        }
        public async Task<ApiKeys> FindOneByIDAsync(string appid)
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"SELECT `AppID`, `ApiKey` FROM `ApiKeys` WHERE `AppID` = @appid";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@appid",
                DbType = DbType.String,
                Value = appid,
            });
            var result = await ReadAllAsync(await cmd.ExecuteReaderAsync());
            return result.Count > 0 ? result[0] : null;
        }
        private async Task<List<ApiKeys>> ReadAllAsync(DbDataReader reader)
        {
            var keys = new List<ApiKeys>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var key = new ApiKeys(Db)
                    {
                        AppID = reader.GetString(0),
                        ApiKey = reader.GetString(1),
                    };
                    keys.Add(key);
                }
            }
            return keys;
        }
    }
}
