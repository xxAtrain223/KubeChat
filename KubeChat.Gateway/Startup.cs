//using k8s;
//using k8s.Models;
//using KubeChat.Gateway.Agones;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Microsoft.ReverseProxy.Service.Proxy;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ReverseProxy.Service.Proxy;
using KubeChat.Gateway.Agones;
using Microsoft.ReverseProxy.Service.RuntimeModel.Transforms;
using System.Collections.Generic;

namespace KubeChat.Gateway
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment WebHostEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            WebHostEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpProxy();

            if (WebHostEnvironment.IsDevelopment())
            {
                services.AddSingleton<IGameServerWatcher>(serviceProvider =>
                {
                    return new FakeGameServerWatcher(new Dictionary<string, GameServerAddress>
                    {
                        {
                            "test",
                            new GameServerAddress
                            {
                                Name = "test",
                                Address = "127.0.0.1",
                                Ports = new Dictionary<string, GameServerStatusPort>
                                {
                                    {
                                        "default",
                                        new GameServerStatusPort
                                        {
                                            Name = "default",
                                            Port = 5002
                                        }
                                    }
                                }
                            }
                        }
                    });
                });
            }
            else
            {
                services.AddSingleton<IGameServerWatcher, GameServerWatcher>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILogger<Startup> logger, IHttpProxy httpProxy, IGameServerWatcher gameServerWatcher, IHostApplicationLifetime hostApplicationLifetime)
        {
            var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false
            });

            hostApplicationLifetime.ApplicationStarted.Register(() =>
            {
                logger.LogDebug("Debug Message");
            });

            var transformer = new CustomTransformer();
            var requestOptions = new RequestProxyOptions(TimeSpan.FromSeconds(100), null);

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/server/{name}/{port}/{**slug}", async httpContext =>
                {
                    var serverName = httpContext.Request.RouteValues["name"].ToString();
                    var portName = httpContext.Request.RouteValues["port"].ToString();
                    var slug = httpContext.Request.RouteValues["slug"]?.ToString();
                    var rawQueryString = httpContext.Request.QueryString;

                    if (!gameServerWatcher.GameServerAddresses.TryGetValue(serverName, out GameServerAddress server))
                    {
                        var errorMessage = $"Server '{serverName}' not found";
                        logger.LogWarning(errorMessage);
                        httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await httpContext.Response.WriteAsync(errorMessage);
                        return;
                    }

                    if (!server.Ports.TryGetValue(portName, out GameServerStatusPort port))
                    {
                        var errorMessage = $"Port '{portName}' for Server '{serverName}' not found";
                        logger.LogWarning(errorMessage);
                        httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await httpContext.Response.WriteAsync(errorMessage);
                        return;
                    }

                    string sourceUri = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
                    //string redirectUri = $"{httpContext.Request.Scheme}://{server.Address}:{port.Port}/{slug}{httpContext.Request.QueryString}";
                    string redirectUri = $"{httpContext.Request.Scheme}://{server.Address}:{port.Port}/";

                    logger.LogDebug($"Redirected {sourceUri} => {redirectUri}{slug}{httpContext.Request.QueryString}");

                    await httpProxy.ProxyAsync(httpContext, redirectUri, httpClient, requestOptions, transformer);
                    var errorFeature = httpContext.Features.Get<IProxyErrorFeature>();
                    if (errorFeature != null)
                    {
                        logger.LogError(errorFeature.Exception, errorFeature.Error.ToString());
                    }
                });
            });
        }

        private class CustomTransformer : HttpTransformer
        {
            public override async Task TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix)
            {
                await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix);
                var pathSegments = httpContext.Request.Path.Value.Split('/');
                httpContext.Request.Path = "/" + string.Join('/', pathSegments[4..]);
            }
        }
    }
}
