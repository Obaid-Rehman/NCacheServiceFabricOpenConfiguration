using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json.Linq;

namespace NCacheDiscovery.Controllers
{
    [Route("api/[controller]")]
    public class NCacheServiceController : Controller
    {
        private readonly ConfigSettings _configSettings;
        private readonly StatelessServiceContext _serviceContext;

        public NCacheServiceController(StatelessServiceContext serviceContext, ConfigSettings configSettings)
        {
            _configSettings = configSettings;
            _serviceContext = serviceContext;
        }


        [HttpGet]
        [Route("/")]
        public async Task<IActionResult> GetCachesAsync()
        {
            var dictionary = await Utilities.GetEndpointsInfo(_serviceContext, _configSettings);

            return Json(dictionary);
        }

    }
}