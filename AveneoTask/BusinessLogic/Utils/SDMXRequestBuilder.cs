using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveneoTask.BusinessLogic.Utils
{
    public static class SDMXRequestBuilder
    {
        private static string Uri = "https://sdw-wsrest.ecb.europa.eu/service/data/EXR/";

        public static string buildRequest(Tuple<string,string> currencyCodes,DateTime startDate,DateTime endDate)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Uri);
            sb.Append("D.");
            sb.Append(currencyCodes.Item1);
            sb.Append(".");
            sb.Append(currencyCodes.Item2);
            sb.Append(".SP00.A");
            sb.Append("?startPeriod=");
            sb.Append(startDate.ToString("yyyy-MM-dd"));
            sb.Append("&endPeriod=");
            sb.Append(endDate.ToString("yyyy-MM-dd"));
            return sb.ToString();
        }



    }
}
