using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AveneoTask.ServiceLayer.ClientFactory
{
    public interface IHttpClientsFactory
    {
        Dictionary<string, HttpClient> Clients();
        HttpClient Client(string key);
    }
}
