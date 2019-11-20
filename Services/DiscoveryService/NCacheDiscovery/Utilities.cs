using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

namespace NCacheDiscovery
{
    internal static class Utilities
    {
        public static async Task<Dictionary<string, List<string>>> GetEndpointsInfo(StatelessServiceContext context, ConfigSettings configSettings)
        {
            string serviceUri = $"{context.CodePackageActivationContext.ApplicationName}/{configSettings.NCacheServiceName}";


            ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

            ResolvedServicePartition resolvedServicePartition = await resolver.ResolveAsync(new Uri(serviceUri), new ServicePartitionKey(), new System.Threading.CancellationToken());

            var endpoints = resolvedServicePartition.Endpoints;
            var endpointDictionary =
                new Dictionary<string, List<string>>();

            JObject addresses;
         //   string web_management_address;
            string bridge_management_address;
            string cache_management_address;
            string cache_client_address;
            string bridge_client_address;

            foreach (var endpoint1 in endpoints)
            {
                addresses = JObject.Parse(endpoint1.Address);
          //      web_management_address = (string)addresses["Endpoints"]["web-management"];
                bridge_management_address = (string)addresses["Endpoints"]["bridge-management"];
                cache_management_address = (string)addresses["Endpoints"]["cache-management"];
                cache_client_address = (string)addresses["Endpoints"]["cache-client"];
                bridge_client_address = (string)addresses["Endpoints"]["bridge-client"];

                //if (!endpointDictionary.ContainsKey("web-management"))
                //{
                //    endpointDictionary["web-management"] = new List<string>();
                //}
                if (!endpointDictionary.ContainsKey("bridge-management"))
                {
                    endpointDictionary["bridge-management"] = new List<string>();
                }
                if (!endpointDictionary.ContainsKey("cache-management"))
                {
                    endpointDictionary["cache-management"] = new List<string>();
                }
                if (!endpointDictionary.ContainsKey("cache-client"))
                {
                    endpointDictionary["cache-client"] = new List<string>();
                }
                if (!endpointDictionary.ContainsKey("bridge-client"))
                {
                    endpointDictionary["bridge-client"] = new List<string>();
                }

           //     endpointDictionary["web-management"].Add(web_management_address);
                endpointDictionary["bridge-management"].Add(bridge_management_address);
                endpointDictionary["cache-management"].Add(cache_management_address);
                endpointDictionary["cache-client"].Add(cache_client_address);
                endpointDictionary["bridge-client"].Add(bridge_client_address);
            }

            return endpointDictionary;
        }
    }
}
