using AveneoTask.ServiceLayer.ClientFactory;
using AveneoTask.ServiceLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AveneoTask.ServiceLayer.Controllers
{
    [ApiController]
    [Route("GenerateKey")]
    public 
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        //private IHttpClientsFactory _httpClientFactory;
        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        public IActionResult GenerateKey(GenerateKeyRequest req)he

        {
            string APIKey = string.Empty;
            GenerateKeyResponse response = new GenerateKeyResponse();
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] secretKeyByteArray = new byte[32]; //256 bit
                cryptoProvider.GetBytes(secretKeyByteArray);
                APIKey = Convert.ToBase64String(secretKeyByteArray);
            }
            string AppID = string.Empty;
            if (!req.forSession)
            { 
            string x64Result = string.Empty;
            string x86Result = string.Empty;
            RegistryKey keyBaseX64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey keyBaseX86 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            RegistryKey keyX64 = keyBaseX64.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
            RegistryKey keyX86 = keyBaseX86.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree);
            object resultObjX64 = keyX64.GetValue("MachineGuid", (object)"default");
            object resultObjX86 = keyX86.GetValue("MachineGuid", (object)"default");
            keyX64.Close();
            keyX86.Close();
            keyBaseX64.Close();
            keyBaseX86.Close();
            keyX64.Dispose();
            keyX86.Dispose();
            keyBaseX64.Dispose();
            keyBaseX86.Dispose();
            keyX64 = null;
            keyX86 = null;
            keyBaseX64 = null;
            keyBaseX86 = null;
            AppID = resultObjX64.ToString();
            }
            else
            {
                AppID = Guid.NewGuid().ToString();
            }
            response.AppID = AppID;
            response.ApiKey = APIKey;
            return Ok(response);
            

        }
    }
}
