using k8s;
using KubeChat.Agones.Kubernetes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KubeChat.Agones.Kubernetes
{
    public interface IGameServerWatcher
    {
        public void Register(Guid requestId, Action<GameServerAddress> gameServerAdded, Action<GameServerAddress> gameServerRemoved);
        public void Unregister(Guid requestId);
    }

    public class GameServerWatcherBase : IGameServerWatcher
    {
        protected ConcurrentDictionary<Guid, GetGameServerRequest> Requests = new ConcurrentDictionary<Guid, GetGameServerRequest>();
        protected ConcurrentDictionary<string, GameServerAddress> GameServerAddresses = new ConcurrentDictionary<string, GameServerAddress>();

        public void Register(Guid requestId, Action<GameServerAddress> gameServerAdded, Action<GameServerAddress> gameServerRemoved)
        {
            var newRequest = new GetGameServerRequest
            {
                RequestId = requestId,
                GameServerAdded = gameServerAdded,
                GameServerRemoved = gameServerRemoved
            };

            if (Requests.TryAdd(requestId, newRequest))
            {
                foreach (var gameServer in GameServerAddresses)
                {
                    gameServerAdded(gameServer.Value);
                }
            }
        }

        public void Unregister(Guid requestId)
        {
            _ = Requests.TryRemove(requestId, out _);
        }
    }

    public class GameServerWatcher : GameServerWatcherBase
    {
        private readonly KubernetesClientConfiguration _kubernetesConfig;
        private readonly k8s.Kubernetes _kubernetesClient;
        private Task<Microsoft.Rest.HttpOperationResponse<object>> _watcher;

        private readonly ILogger<GameServerWatcher> _logger;

        public GameServerWatcher(ILogger<GameServerWatcher> logger)
        {
            _kubernetesConfig = KubernetesClientConfiguration.InClusterConfig();
            _kubernetesClient = new k8s.Kubernetes(_kubernetesConfig);

            _logger = logger;

            ConnectToKubernetes();
        }

        private void ConnectToKubernetes()
        {
            _watcher = _kubernetesClient.ListNamespacedCustomObjectWithHttpMessagesAsync("agones.dev", "v1", "kubechat", "gameservers", watch: true);
            _watcher.Watch(
                (Action<WatchEventType, object>)((watchEvent, resourceObject) =>
                {
                    var server = (resourceObject as JObject).ToObject<GameServer>();

                    if (server.Status.State == GameServerState.Allocated &&
                       (watchEvent == WatchEventType.Added ||
                        watchEvent == WatchEventType.Modified))
                    {
                        AddGameServer(server);
                    }
                    else if (server.Status.State == GameServerState.Allocated &&
                        watchEvent == WatchEventType.Deleted)
                    {
                        RemoveGameServer(server);
                    }
                }),
                (e) => // onError
                {
                    _logger.LogError(e, "Kubernetes error");
                },
                () => // onClosed
                {
                    ConnectToKubernetes();
                });
        }

        private void AddGameServer(GameServer server)
        {
            var newGameServer = new GameServerAddress
            {
                Name = server.Metadata.Name,
                Address = server.Status.Address,
                Ports = server.Status.Ports.ToDictionary(p => p.Name)
            };

            if (GameServerAddresses.TryAdd(server.Metadata.Name, newGameServer))
            {
                _logger.LogInformation($"Added GameServer '{server.Metadata.Name}' with Ports: {string.Join(", ", server.Status.Ports.Select(p => $"'{p.Name}'"))}");

                foreach (var request in Requests.Values)
                {
                    request.GameServerAdded(newGameServer);
                }
            }
        }

        private void RemoveGameServer(GameServer server)
        {
            if (GameServerAddresses.TryRemove(server.Metadata.Name, out var gameServer))
            {
                _logger.LogInformation($"Removed GameServer '{server.Metadata.Name}'");

                foreach (var request in Requests.Values)
                {
                    request.GameServerRemoved(gameServer);
                }
            }
        }
    }

    public class FakeGameServerWatcher : GameServerWatcherBase
    {
        public FakeGameServerWatcher(ConcurrentDictionary<string, GameServerAddress> gameServerAddresses) =>
            GameServerAddresses = gameServerAddresses;
    }

    public class GameServerAddress
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public IDictionary<string, GameServerStatusPort> Ports { get; set; }
    }

    public class GetGameServerRequest
    {
        public Guid RequestId { get; set; }
        public Action<GameServerAddress> GameServerAdded { get; set; }
        public Action<GameServerAddress> GameServerRemoved { get; set; }
    }

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
}
