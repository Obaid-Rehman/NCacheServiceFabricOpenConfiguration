using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Alachisoft.NCache.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace StatelessASPNetCoreClient
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddControllersAsServices();

            services.AddSingleton<ICache>(sp =>
            {
                var configSettings = sp.GetRequiredService<ConfigSettings>();
                var discoveryServiceUri = configSettings.NCacheDiscoveryService;

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = httpClient.GetAsync(discoveryServiceUri).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(content);

                        var ncacheClientEndpoints = dictionary["cache-client"];

                        if (ncacheClientEndpoints.Count == 0)
                        {
                            throw new Exception("No cache IP addresses");
                        }

                        List<ServerInfo> cacheServers = new List<ServerInfo>();
                        string ipAddress = "";
                        int port = -1;
                        foreach (var ncacheClientEndpoint in ncacheClientEndpoints)
                        {
                            var endpointInfo = ncacheClientEndpoint.Replace("tcp://", "").Split(':');
                            ipAddress = endpointInfo[0];
                            port = int.Parse(endpointInfo[1].Trim());

                            cacheServers.Add(new ServerInfo(ipAddress, port));
                        }

                        return CacheManager.GetCache("demoCache", new CacheConnectionOptions
                        {
                            ServerList = cacheServers
                        });
                    }
                    else
                    {
                        throw new HttpRequestException($"StatusCode:{response.StatusCode}.Reason: {response.ReasonPhrase}");
                    }
                }

            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Stateless ASP.NET Core Client", Version = "v1" });
            });


            services.AddCors(
                options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder.SetIsOriginAllowed((host) => true)
                                            .AllowAnyMethod()
                                            .AllowAnyHeader()
                                            .AllowCredentials()
                        );
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            app.UseStaticFiles();
            app.UseCors("CorsPolicy");

            app.UseMvc(
                //routes =>
                //{
                //    routes.MapRoute(
                //        name: "default",
                //        template: "{controller=NCacheService}/{action=Index}");
                //}
                );

            app.UseSwagger();
            app.UseSwaggerUI(c =>
           {
               c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stateless ASP.NET Core Client V1");
               c.RoutePrefix = string.Empty;
           });
        }
    }
}
