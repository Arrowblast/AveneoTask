using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AveneoTask.BusinessLogic.Modules;
using AveneoTask.Database;
using AveneoTask.Database.Query;
using AveneoTask.Security;
using AveneoTask.ServiceLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AveneoTask.Controllers
{
    [HMACAuthentication]
    [ApiController]
    [Route("ExchangeRate")]
    public class ExchangeRateFromDatesController : ControllerBase
    {
        private MySQLDB Db;
        private readonly ILogger<ExchangeRateFromDatesController> _logger;
        private IMemoryCache _cache;
        public ExchangeRateFromDatesController(ILogger<ExchangeRateFromDatesController> logger, IMemoryCache cache,MySQLDB db)
        {
            _logger = logger;
            _cache = cache;
            Db = db;
        }

        [HttpPost]
        [Route("GetFromDates")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None, NoStore = false)]
        public async Task<IActionResult> GetData(ExchangeRateFromDatesRequest req)
        {
            _logger.LogInformation("/ExchangeRate/GetFromDates");
            try
            {
                if (req.ApiKey == _cache.Get("ApiKey") as string)
                {
                    
                    var result = await new ExchangeRateFromDatesBL().GetData(req, _cache);

                    return Ok(JsonConvert.SerializeObject(result));
                }
                else
                {
                    await Db.Connection.OpenAsync();
                    var query = new ApiKeysQuery(Db);
                    var queryResult = await query.FindOneByKeyAsync(req.ApiKey);
                    if(queryResult is null)
                    {
                        await Db.Connection.CloseAsync();
                        return BadRequest("Nieprawidłowy klucz");
                    }
                    else
                    {
                        var result = await new ExchangeRateFromDatesBL().GetData(req, _cache);
                        await Db.Connection.CloseAsync();
                        return Ok(JsonConvert.SerializeObject(result));
                    }
                    
                }
            }
            catch(Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
