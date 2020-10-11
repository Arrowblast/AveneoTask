using AveneoTask.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AveneoTask.ServiceLayer.ClientFactory
{
    public class HttpClientsFactory : IHttpClientsFactory
    {
        public static Dictionary<string, HttpClient> HttpClients { get; set; }
        private readonly ILogger _logger;
       // private readonly ServicePointsOptions _serverOptionsAccessor;

        public HttpClientsFactory(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpClientsFactory>();
         //   _serverOptionsAccessor = serverOptionsAccessor;
            HttpClients = new Dictionary<string, HttpClient>();
            Initialize();
        }

        private void Initialize()
        {
            HttpClient client = HttpClientFactory.Create(new AuthDelegatingHandler());
          
            // ADD apiServer
            var apiServer = "https://localhost:8001";
            client.BaseAddress = new Uri(apiServer);
            HttpClients.Add("apiServer", client);
        }

        public Dictionary<string, HttpClient> Clients()
        {
            return HttpClients;
        }

        public HttpClient Client(string key)
        {
            return Clients()[key];
        }
    }
}
