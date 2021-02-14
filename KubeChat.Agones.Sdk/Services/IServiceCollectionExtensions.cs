using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;

namespace KubeChat.Agones.Services
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAgonesGrpcClient(this IServiceCollection services, string agonesEndpoint)
        {
            services.AddGrpcClient<Agones.AgonesClient>(o =>
            {
                o.Address = new Uri(agonesEndpoint);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
                new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    EnableMultipleHttp2Connections = true
                });

            return services;
        }

        public static IServiceCollection AddGameServerWatcher(this IServiceCollection services)
        {
            services.AddSingleton<GameServerServices>();
            services.AddHostedService(serviceProvider => serviceProvider.GetService<GameServerServices>());
            return services;
        }
    }
}
