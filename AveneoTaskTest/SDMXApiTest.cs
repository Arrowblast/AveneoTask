using AveneoTask.BusinessLogic.Modules;
using AveneoTask.BusinessLogic.Utils;
using AveneoTask.ServiceLayer.Models;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace AveneoTaskTest
{
    public class SDMXApiTest
    {
        private ExchangeRateFromDatesBL erBL;
        [SetUp]
        public void Setup()
        {
            erBL = new ExchangeRateFromDatesBL();
        }

        [Test]
        public async Task CheckDataFromCertainDayAsync()
        {
            Assert.NotZero(await erBL.getValueFromSingularDay(new Tuple<string, string>("USD", "EUR"), new DateTime(2009, 5, 5)));
        }
        [Test]
        public async Task DataNotFoundByApi()
        {
            Assert.IsEmpty(await this.ApiDataGeneration());
        }
        [Test]
        public async Task ApiCorrectionWithFindingData()
        {
            Assert.IsEmpty(await this.ApiDataGeneration());
            Assert.NotZero(await erBL.getValueFromSingularDay(new Tuple<string, string>("USD", "EUR"), new DateTime(2009, 5, 2)));
        }
        [Test] 
        public async Task EmptyDaysFilled()
        {
            HttpClient httpClient = new HttpClient();
            ExchangeRateFromDatesSDMXResponse SDMXresponse = new ExchangeRateFromDatesSDMXResponse();
            Tuple<string, string> currencyPair = new Tuple<string, string>("USD", "EUR");
            DateTime startDate = new DateTime(2009, 5, 5);
            DateTime endDate = new DateTime(2009, 5, 30);
            string SDMXrequest = SDMXRequestBuilder.buildRequest(currencyPair, startDate, endDate);
            XmlDocument response = new XmlDocument();
            var nsmgr = new XmlNamespaceManager(response.NameTable);
            nsmgr.AddNamespace("message", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message");
            nsmgr.AddNamespace("generic", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic");
            SDMXresponse.ExchangeRateName = "USD/EUR";
            var getData = await httpClient.GetAsync(SDMXrequest);
            var parseDataToString = await getData.Content.ReadAsStringAsync();
            if (parseDataToString != string.Empty)
            {
                response.LoadXml(parseDataToString);
                XmlNodeList exchangeRateValues = response.SelectNodes("//message:GenericData/message:DataSet/generic:Series/generic:Obs", nsmgr);
                foreach (XmlNode element in exchangeRateValues)
                {
                    XmlNode dateNode = element.ChildNodes[0];
                    DateTime dateTime = DateTime.Parse(dateNode.Attributes.GetNamedItem("value").Value);
                    XmlNode valueNode = element.ChildNodes[1];
                    Decimal exchangeValue = 0;
                    SDMXresponse.Values.Add(dateTime, exchangeValue);
                }
            }
            var dataCountBefore = SDMXresponse.Values.Count;
            SDMXresponse = await erBL.checkData(SDMXresponse, startDate, endDate);
            Assert.Greater(SDMXresponse.Values.Count, dataCountBefore);


        }
        private async Task<string> ApiDataGeneration()
        {
            HttpClient httpClient = new HttpClient();
            DateTime date = new DateTime(2009, 5, 2);
            Tuple<string, string> currencyCodes = new Tuple<string, string>("USD", "EUR");
            string SDMXrequest = SDMXRequestBuilder.buildRequest(currencyCodes, date, date);
            XmlDocument response = new XmlDocument();
            var nsmgr = new XmlNamespaceManager(response.NameTable);
            nsmgr.AddNamespace("message", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/message");
            nsmgr.AddNamespace("generic", "http://www.sdmx.org/resources/sdmxml/schemas/v2_1/data/generic");
            var getData = await httpClient.GetAsync(SDMXrequest);
            var parseDataToString = await getData.Content.ReadAsStringAsync();
            return parseDataToString;
        }
    }
}