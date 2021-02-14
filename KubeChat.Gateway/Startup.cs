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
using System.Collections.Generic;
using Grpc.Net.Client;
using Polly;
using System.Threading;
using Grpc.Core;
using System.Linq;
using KubeChat.Agones.Services;

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

            services.AddAgonesGrpcClient(Configuration.GetValue<string>("KubeChat.Agones"))
                .AddGameServerWatcher();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILogger<Startup> logger, IHttpProxy httpProxy, GameServerServices gameServerWatcher)
        {
            var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false
            });

            var transformer = new RemoveServerParametersTransformer();
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

                    if (!gameServerWatcher.GameServerAddresses.TryGetValue(serverName, out var server))
                    {
                        var errorMessage = $"Server '{serverName}' not found";
                        logger.LogWarning(errorMessage);
                        httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await httpContext.Response.WriteAsync(errorMessage);
                        return;
                    }

                    if (!server.Ports.TryGetValue(portName, out var port))
                    {
                        var errorMessage = $"Port '{portName}' for Server '{serverName}' not found";
                        logger.LogWarning(errorMessage);
                        httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await httpContext.Response.WriteAsync(errorMessage);
                        return;
                    }

                    string sourceUri = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
                    string redirectUri = $"{httpContext.Request.Scheme}://{server.Address}:{port.Number}/";

                    logger.LogInformation($"Redirected client {httpContext.Connection.RemoteIpAddress} from {sourceUri} to {redirectUri}{slug}{httpContext.Request.QueryString}");

                    await httpProxy.ProxyAsync(httpContext, redirectUri, httpClient, requestOptions, transformer);
                    var errorFeature = httpContext.Features.Get<IProxyErrorFeature>();
                    if (errorFeature != null)
                    {
                        logger.LogError(errorFeature.Exception, errorFeature.Error.ToString());
                    }
                });
            });
        }

        private class RemoveServerParametersTransformer : HttpTransformer
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
