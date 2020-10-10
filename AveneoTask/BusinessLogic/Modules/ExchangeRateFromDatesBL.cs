using AveneoTask.BusinessLogic.Utils;
using AveneoTask.ServiceLayer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace AveneoTask.BusinessLogic.Modules
{
    public class ExchangeRateFromDatesBL
    {
        HttpClient httpClient = new HttpClient();
        
        public async Task<ExchangeRateFromDatesResponse> GetData(ExchangeRateFromDatesRequest req)
        {
            ExchangeRateFromDatesResponse exchangeRateFromDatesResponse = new ExchangeRateFromDatesResponse();
            foreach(string key in req.CurrencyCodes.Keys)
            {
                ExchangeRateFromDatesSDMXResponse SDMXresponse = new ExchangeRateFromDatesSDMXResponse();
                string SDMXrequest = SDMXRequestBuilder.buildRequest(new Tuple<string, string>(key, req.CurrencyCodes[key]), req.StartDate, req.EndDate);
                XmlDocument response = new XmlDocument();
                var nsmgr = new XmlNamespaceManager(response.NameTable);
                nsmgr.AddNamespace("message", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message");
                nsmgr.AddNamespace("generic", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic");
                SDMXresponse.ExchangeRateName = key + "/" + req.CurrencyCodes[key];
                var getData = await httpClient.GetAsync(SDMXrequest);
                if(getData.StatusCode.ToString()!="OK")
                {
                    SDMXresponse.Status = getData.ReasonPhrase;
                    exchangeRateFromDatesResponse.ExchangeRateValues.Add(SDMXresponse);
                    continue;
                }
                var parseDataToString = await getData.Content.ReadAsStringAsync();
                response.LoadXml(parseDataToString);
                XmlNodeList exchangeRateValues = response.SelectNodes("//message:GenericData/message:DataSet/generic:Series/generic:Obs",nsmgr);
               
                foreach (XmlNode element in exchangeRateValues)
                {
                    XmlNode dateNode = element.ChildNodes[0];
                    DateTime dateTime = DateTime.Parse(dateNode.Attributes.GetNamedItem("value").Value);
                    XmlNode valueNode = element.ChildNodes[1];
                    Decimal exchangeValue = Decimal.Parse(valueNode.Attributes.GetNamedItem("value").Value, CultureInfo.InvariantCulture);
                    
                    SDMXresponse.Values.Add(dateTime, exchangeValue);
                } 
                exchangeRateFromDatesResponse.ExchangeRateValues.Add(SDMXresponse);
            }
            return exchangeRateFromDatesResponse;
        }

    }
}
