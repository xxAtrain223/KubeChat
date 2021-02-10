using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Agones;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace KubeChat.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddCors();

            // Add a singleton service to communicate with Agones
            services.AddSingleton((IServiceProvider serviceProvider) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();
                var agones = new AgonesSDK();

                try
                {
                    var connected = agones.ConnectAsync().Result;
                    if (!connected)
                    {
                        logger.LogCritical("Agones did not connect.");
                    }
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, "Agones did not connect with exception.");
                }

                return agones;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostAppLifetime, ILogger<Startup> logger)
        {
            logger.LogInformation($"KubernetesResource: {Configuration.GetValue<string>("KubernetesResource")}");
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Only start the Agones health ping if running an Agones GameServer
            if (Configuration.GetValue<string>("KubernetesResource") == "GameServer")
            {
                hostAppLifetime.ApplicationStarted.Register(async () =>
                {
                    var agones = app.ApplicationServices.GetRequiredService<AgonesSDK>();

                    // Alert Agones that this server is ready once the application has finished starting
                    await agones.ReadyAsync();

                    // Keep sending health pings until Shutdown
                    var running = true;

                    // Send health ping every third of a period, to allow for at least a second try.
                    var healthPeriod = TimeSpan.FromSeconds((await agones.GetGameServerAsync())
                        .Spec.Health.PeriodSeconds) / 3;

                    // Setup a callback to watch for relevant changes
                    agones.WatchGameServer((Agones.Dev.Sdk.GameServer gameServer) =>
                    {
                        running = gameServer.Status.State != "Shutdown";
                        healthPeriod = TimeSpan.FromSeconds(gameServer.Spec.Health.PeriodSeconds) / 3;
                    });

                    // Start a thread to send health pings
                    new Thread(async () =>
                    {
                        while (running)
                        {
                            await agones.HealthAsync();
                            Thread.Sleep(healthPeriod);
                        }
                    }).Start();
                });
            }

            app.UseCors(builder =>
            {
                if (env.IsDevelopment())
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .WithMethods("GET", "POST");
                }
                else
                {
                    builder.WithOrigins("http://serverside.client.kubechat.com")
                        .AllowAnyHeader()
                        .WithMethods("GET", "POST")
                        .AllowCredentials();
                    builder.WithOrigins("http://webassembly.client.kubechat.com")
                        .AllowAnyHeader()
                        .WithMethods("GET", "POST")
                        .AllowCredentials();
                }
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<Hubs.ChatHub>("/ChatHub");
            });
        }
    }
}
