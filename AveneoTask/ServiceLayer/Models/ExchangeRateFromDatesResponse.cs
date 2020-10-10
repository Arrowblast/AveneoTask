using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AveneoTask.ServiceLayer.Models
{
    public class ExchangeRateFromDatesResponse
    {
        public ExchangeRateFromDatesResponse()
        {
            ExchangeRateValues = new List<ExchangeRateFromDatesSDMXResponse>();
        }
        public List<ExchangeRateFromDatesSDMXResponse> ExchangeRateValues { get; set; }
    }
}
