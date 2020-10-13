using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AveneoTask.ServiceLayer.Models
{
    public class ExchangeRateFromDatesSDMXResponse
    {
        public ExchangeRateFromDatesSDMXResponse()
        {
            Values = new SortedDictionary<DateTime, decimal>();
        }
        public string ExchangeRateName { get; set; }
        public SortedDictionary<DateTime, decimal> Values { get; set; }
        public string Status { get; set; }
    }
}
