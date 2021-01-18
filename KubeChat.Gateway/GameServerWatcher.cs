using k8s;
using KubeChat.Gateway.Agones;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KubeChat.Gateway
{
    public interface IGameServerWatcher
    {
        public IReadOnlyDictionary<string, GameServerAddress> GameServerAddresses { get; }
    }

    public class GameServerWatcher : IGameServerWatcher
    {
        public IReadOnlyDictionary<string, GameServerAddress> GameServerAddresses  => _gameServerAddresses; 

        private readonly Dictionary<string, GameServerAddress> _gameServerAddresses;
        private readonly KubernetesClientConfiguration _kubernetesConfig;
        private readonly Kubernetes _kubernetesClient;
        private Task<Microsoft.Rest.HttpOperationResponse<object>> _watcher;
        private ILogger<GameServerWatcher> _logger;

        public GameServerWatcher(ILogger<GameServerWatcher> logger)
        {
            _gameServerAddresses = new Dictionary<string, GameServerAddress>();
            _kubernetesConfig = KubernetesClientConfiguration.InClusterConfig();
            _kubernetesClient = new Kubernetes(_kubernetesConfig);
            _logger = logger;

            Connect();
        }

        private void Connect()
        {
            _watcher = _kubernetesClient.ListNamespacedCustomObjectWithHttpMessagesAsync("agones.dev", "v1", "kubechat", "gameservers", watch: true);
            _watcher.Watch<object, object>(
                (watchEvent, resourceObject) =>
                {
                    var server = (resourceObject as JObject).ToObject<GameServer>();

                    if (server.Status.State == GameServerState.Allocated &&
                       (watchEvent == WatchEventType.Added ||
                        watchEvent == WatchEventType.Modified))
                    {
                        _gameServerAddresses.Add(server.Metadata.Name,
                            new GameServerAddress
                            {
                                Name = server.Metadata.Name,
                                Address = server.Status.Address,
                                Ports = server.Status.Ports.ToDictionary(p => p.Name)
                            });
                        _logger.LogInformation($"Added GameServer '{server.Metadata.Name}' with Ports: {string.Join(", ", server.Status.Ports.Select(p => $"'{p.Name}'"))}");
                    }
                    else if (server.Status.State == GameServerState.Allocated &&
                        watchEvent == WatchEventType.Deleted)
                    {
                        _gameServerAddresses.Remove(server.Metadata.Name);
                        _logger.LogInformation($"Removed GameServer '{server.Metadata.Name}'");
                    }
                },
                (e) => // onError
                {
                    _logger.LogError(e, "Kubernetes error");
                },
                () => // onClosed
                {
                    _gameServerAddresses.Clear();
                    Connect();
                });
        }
    }

    public class FakeGameServerWatcher : IGameServerWatcher
    {
        private IDictionary<string, GameServerAddress> _gameServerAddresses;

        public FakeGameServerWatcher(IDictionary<string, GameServerAddress> gameServerAddresses) =>
            _gameServerAddresses = gameServerAddresses;

        public IReadOnlyDictionary<string, GameServerAddress> GameServerAddresses =>
            (IReadOnlyDictionary<string, GameServerAddress>)_gameServerAddresses;
    }

    public class GameServerAddress
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public IDictionary<string, GameServerStatusPort> Ports { get; set; }
    }
}
