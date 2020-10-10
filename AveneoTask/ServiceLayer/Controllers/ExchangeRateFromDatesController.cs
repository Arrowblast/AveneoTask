using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AveneoTask.BusinessLogic.Modules;
using AveneoTask.ServiceLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AveneoTask.Controllers
{
    [ApiController]
    [Route("ExchangeRate")]
    public class ExchangeRateFromDatesController : ControllerBase
    {
        
        private readonly ILogger<ExchangeRateFromDatesController> _logger;

        public ExchangeRateFromDatesController(ILogger<ExchangeRateFromDatesController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("GetFromDates")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None, NoStore = false)]
        public async Task<IActionResult> GetData(ExchangeRateFromDatesRequest req)
        {
            try
            {
                
                var result = await new ExchangeRateFromDatesBL().GetData(req);
                
                return Ok(JsonConvert.SerializeObject(result));
            }
            catch(Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
