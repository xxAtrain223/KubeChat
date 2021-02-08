using KubeChat.Agones.Kubernetes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KubeChat.Agones.Services;

namespace KubeChat.Agones
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddGameServerWatcher(this IServiceCollection services, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                return services.AddSingleton<IGameServerWatcher>(serviceProvider =>
                {
                    var gameServerStatusPort = new GameServerStatusPort
                    {
                        Name = "default",
                        Number = 5000
                    };

                    var gameServerStatusPorts = new Dictionary<string, GameServerStatusPort>
                    {
                        { gameServerStatusPort.Name, gameServerStatusPort }
                    };

                    var gameServerAddress = new GameServerAddress
                    {
                        Name = "test",
                        Address = "127.0.0.1",
                        Ports = gameServerStatusPorts
                    };

                    var gameServerAddresses = new ConcurrentDictionary<string, GameServerAddress>();
                    _ = gameServerAddresses.TryAdd(gameServerAddress.Name, gameServerAddress);

                    return new FakeGameServerWatcher(gameServerAddresses);
                });
            }
            else
            {
                return services.AddSingleton<IGameServerWatcher, GameServerWatcher>();
            }
        }
    }

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
            services.AddGrpc();

            services.AddGameServerWatcher(WebHostEnvironment);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (WebHostEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<AgonesService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
