
using AveneoTask.Database;
using AveneoTask.Database.Models;
using AveneoTask.Database.Query;
using AveneoTask.ServiceLayer.ClientFactory;
using AveneoTask.ServiceLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AveneoTask.ServiceLayer.Controllers
{
    [ApiController]
    [Route("GenerateKey")]
    public class AuthController : ControllerBase
    {
        private MySQLDB Db;
        private readonly ILogger<AuthController> _logger;
        //private IHttpClientsFactory _httpClientFactory;
        private IMemoryCache _cache;
        public AuthController(ILogger<AuthController> logger, IMemoryCache cache, MySQLDB db)
        {
            _logger = logger;
            _cache = cache;
            Db = db;
        }
        [HttpPost]
        public async Task<IActionResult> GenerateKey(GenerateKeyRequest req)
        {
            _logger.LogInformation("/ExchangeRate/GetFromDates");
            string APIKey = string.Empty;
            GenerateKeyResponse response = new GenerateKeyResponse();
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] secretKeyByteArray = new byte[32]; //256 bit
                cryptoProvider.GetBytes(secretKeyByteArray);
                APIKey = Convert.ToBase64String(secretKeyByteArray);
            }
            string AppID = string.Empty;
            if (!req.forSession)
            {
                AppID = GetMachineGuid();
            }
            else
            {
                AppID = Guid.NewGuid().ToString();
            }
            response.AppID = AppID;
            response.ApiKey = APIKey;
            MemoryCacheEntryOptions cacheExpirationOptions = new MemoryCacheEntryOptions();
            cacheExpirationOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(30);
            cacheExpirationOptions.Priority = CacheItemPriority.High;
            _cache.Set("AppID", AppID, cacheExpirationOptions);
            _cache.Set("ApiKey", APIKey, cacheExpirationOptions);
            await Db.Connection.OpenAsync();
            ApiKeys model = new ApiKeys()
            {
                AppID = AppID,
                ApiKey = APIKey
            };
            model.Db = Db;
            var query = new ApiKeysQuery(Db);
            var queryResult = await query.FindOneByIDAsync(AppID);
            if(queryResult is null)
            {
                await model.InsertAsync();
            }
            else
            {
                await model.UpdateAsync();
            }
            await Db.Connection.CloseAsync();
            return Ok(response);
            

        }
        public static string GetMachineGuid()
        {
            string x64Result = string.Empty;
            string x86Result = string.Empty;
            RegistryKey keyBaseX64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey keyBaseX86 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            RegistryKey keyX64 = keyBaseX64.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
            RegistryKey keyX86 = keyBaseX86.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
            object resultObjX64 = keyX64.GetValue("MachineGuid", (object)"default");
            object resultObjX86 = keyX86.GetValue("MachineGuid", (object)"default");
            keyX64.Close();
            keyX86.Close();
            keyBaseX64.Close();
            keyBaseX86.Close();
            keyX64.Dispose();
            keyX86.Dispose();
            keyBaseX64.Dispose();
            keyBaseX86.Dispose();
            keyX64 = null;
            keyX86 = null;
            keyBaseX64 = null;
            keyBaseX86 = null;
            return resultObjX64.ToString();
        }
    }
}
