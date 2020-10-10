using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AveneoTask.ServiceLayer.Models
{
    public class ExchangeRateFromDatesRequest
    {
        public Dictionary<string, string> CurrencyCodes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
