using AveneoTask.BusinessLogic.Utils;
using AveneoTask.ServiceLayer.Models;
using Microsoft.Extensions.Caching.Memory;
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
        IMemoryCache _cache;
        public async Task<ExchangeRateFromDatesResponse> GetData(ExchangeRateFromDatesRequest req, IMemoryCache cache)
        {

            _cache = cache;

            if(req.EndDate > DateTime.Now)
            {
                throw new Exception("Nie można pobrać danych z przyszłości");
            }
            ExchangeRateFromDatesResponse exchangeRateFromDatesResponse = new ExchangeRateFromDatesResponse();
            foreach(string key in req.CurrencyCodes.Keys)
            {
                ExchangeRateFromDatesSDMXResponse cachedResponse = new ExchangeRateFromDatesSDMXResponse();
                if (!_cache.TryGetValue(key + "/" + req.CurrencyCodes[key] + "_" + req.StartDate.ToShortDateString() + "_" + req.EndDate.ToShortDateString(), out cachedResponse))
                {
                    ExchangeRateFromDatesSDMXResponse SDMXresponse = new ExchangeRateFromDatesSDMXResponse();
                    string SDMXrequest = SDMXRequestBuilder.buildRequest(new Tuple<string, string>(key, req.CurrencyCodes[key]), req.StartDate, req.EndDate);
                    XmlDocument response = new XmlDocument();
                    var nsmgr = new XmlNamespaceManager(response.NameTable);
                    nsmgr.AddNamespace("message", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message");
                    nsmgr.AddNamespace("generic", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic");
                    SDMXresponse.ExchangeRateName = key + "/" + req.CurrencyCodes[key];
                    var getData = await httpClient.GetAsync(SDMXrequest);
                    if (getData.StatusCode.ToString() != "OK")
                    {
                        SDMXresponse.Status = getData.ReasonPhrase;
                        exchangeRateFromDatesResponse.ExchangeRateValues.Add(SDMXresponse);
                        continue;
                    }
                    var parseDataToString = await getData.Content.ReadAsStringAsync();
                    if (parseDataToString != string.Empty)
                    {
                    response.LoadXml(parseDataToString);
                    XmlNodeList exchangeRateValues = response.SelectNodes("//message:GenericData/message:DataSet/generic:Series/generic:Obs", nsmgr);
                    decimal lastValue = 0;
                    foreach (XmlNode element in exchangeRateValues)
                    {
                        XmlNode dateNode = element.ChildNodes[0];
                        DateTime dateTime = DateTime.Parse(dateNode.Attributes.GetNamedItem("value").Value);
                        XmlNode valueNode = element.ChildNodes[1];
                        Decimal exchangeValue = 0;
                        try
                        {
                            exchangeValue = Decimal.Parse(valueNode.Attributes.GetNamedItem("value").Value, CultureInfo.InvariantCulture);
                        }
                        catch (FormatException ex)
                        {
                            if (lastValue == 0)
                                exchangeValue = await getValueFromSingularDay(new Tuple<string, string>(key, req.CurrencyCodes[key]), Convert.ToDateTime(dateTime).Subtract(TimeSpan.FromDays(1)));
                            else
                                exchangeValue = lastValue;
                        }

                        lastValue = exchangeValue;
                        SDMXresponse.Values.Add(dateTime, exchangeValue);
                    }
                    }
                    SDMXresponse = await checkData(SDMXresponse, req.StartDate, req.EndDate);
                    exchangeRateFromDatesResponse.ExchangeRateValues.Add(SDMXresponse);
                    MemoryCacheEntryOptions cacheExpirationOptions = new MemoryCacheEntryOptions();
                    cacheExpirationOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(30);
                    cacheExpirationOptions.Priority = CacheItemPriority.High;
                    _cache.Set(SDMXresponse.ExchangeRateName + "_" + req.StartDate.ToShortDateString() + "_" + req.EndDate.ToShortDateString(), SDMXresponse, cacheExpirationOptions); 
                }
                else
                {
                    exchangeRateFromDatesResponse.ExchangeRateValues.Add(cachedResponse);
                }
               
            }
            return exchangeRateFromDatesResponse;
        }

        public async Task<decimal> getValueFromSingularDay(Tuple<string, string> currencyCodes, DateTime date)
        {
            try
            {
                string SDMXrequest = SDMXRequestBuilder.buildRequest(currencyCodes, date, date);
                XmlDocument response = new XmlDocument();
                var nsmgr = new XmlNamespaceManager(response.NameTable);
                nsmgr.AddNamespace("message", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message");
                nsmgr.AddNamespace("generic", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic");
                var getData = await httpClient.GetAsync(SDMXrequest);
                var parseDataToString = await getData.Content.ReadAsStringAsync();
                response.LoadXml(parseDataToString);
                XmlNode exchangeRateValue = response.SelectSingleNode("//message:GenericData/message:DataSet/generic:Series/generic:Obs", nsmgr);
                XmlNode valueNode = exchangeRateValue.ChildNodes[1];
                decimal exchangeValue= Decimal.Parse(valueNode.Attributes.GetNamedItem("value").Value, CultureInfo.InvariantCulture);
                return exchangeValue;
            }
            catch(FormatException ex)
            {
                return await getValueFromSingularDay(currencyCodes, Convert.ToDateTime(date).Subtract(TimeSpan.FromDays(1)));
            }
            catch(XmlException xml)
            {
                return await getValueFromSingularDay(currencyCodes, Convert.ToDateTime(date).Subtract(TimeSpan.FromDays(1)));
            }
        }
        public async Task<ExchangeRateFromDatesSDMXResponse> checkData(ExchangeRateFromDatesSDMXResponse SDMXResponse,DateTime startDate,DateTime endDate)
        {
            ExchangeRateFromDatesSDMXResponse responseToCheck = SDMXResponse;
            string[] splittedName = responseToCheck.ExchangeRateName.Split("/");
            Decimal lastValue = 0;
            for(DateTime date = startDate; date<=endDate;date=date.AddDays(1))
            {
                if(!responseToCheck.Values.ContainsKey(date))
                {
                    if(lastValue == 0)
                    {
                        lastValue = await getValueFromSingularDay(new Tuple<string, string>(splittedName[0], splittedName[1]), Convert.ToDateTime(date).Subtract(TimeSpan.FromDays(1)));
                    }
                    responseToCheck.Values.Add(date, lastValue);
                }
                lastValue = responseToCheck.Values[date];
            }

            return responseToCheck;
        }


    }
}
