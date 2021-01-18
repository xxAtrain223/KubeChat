//using k8s;
//using k8s.Models;
//using KubeChat.Gateway.Agones;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Primitives;
//using Microsoft.Rest;
//using Microsoft.ReverseProxy.Abstractions;
//using Microsoft.ReverseProxy.Service;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace KubeChat.Gateway
//{
//    public class KubernetesProxyConfigProvider : IProxyConfigProvider
//    {
//        private volatile KubernetesProxyConfig _config;
//        private readonly ILogger<KubernetesProxyConfigProvider> _logger;
//        private Dictionary<string, GameServersAddress> _gameServerAddresses;

//        public KubernetesProxyConfigProvider(ILogger<KubernetesProxyConfigProvider> logger)
//        {
//            _logger = logger;
//            _gameServerAddresses = new Dictionary<string, GameServersAddress>();

//            var kubernetesConfig = KubernetesClientConfiguration.InClusterConfig();
//            var kubernetesClient = new Kubernetes(kubernetesConfig);

//            var customObjectResponse = kubernetesClient.ListNamespacedCustomObjectWithHttpMessagesAsync("agones.dev", "v1", "kubechat", "gameservers", watch: true);
//            customObjectResponse.Watch<object, object>(
//                (watchEvent, resourceObject) =>
//                {
//                    var server = (resourceObject as JObject).ToObject<GameServer>();

//                    //var eventType = Enum.GetName(watchEvent);
//                    //logger.LogInformation($"Watch: Event: {eventType}\tGameServerName: {server.Metadata.Name}\tStatus: {server.Status.State}");
//                    //logger.LogInformation($"Address: {server.Status.Address}\tPorts: {string.Join(", ", server.Status.Ports.Select(p => $"{p.Name}:{p.Port}"))}");
                    
//                    if (server.Status.State == GameServerState.Allocated &&
//                       (watchEvent == WatchEventType.Added ||
//                        watchEvent == WatchEventType.Modified))
//                    {
//                        _gameServerAddresses.Add(server.Metadata.Name,
//                            new GameServersAddress
//                            {
//                                Name = server.Metadata.Name,
//                                Address = server.Status.Address,
//                                Ports = server.Status.Ports
//                            });
//                    }
//                    else if (server.Status.State == GameServerState.Allocated &&
//                        watchEvent == WatchEventType.Deleted)
//                    {
//                        _gameServerAddresses.Remove(server.Metadata.Name); 
//                    }
//                },
//                (e) => // onError
//                {
//                    logger.LogError(e, "Kubernetes error");
//                },
//                () => // onClosed
//                {
//                    logger.LogError("Kubernetes closed");
//                });
//        }

//        public IProxyConfig GetConfig() => _config;

//        public void Update()
//        {
//            var routes = new List<ProxyRoute>();
//            var clusters = new List<Cluster>();

//            foreach (var server in _gameServerAddresses.Values)
//            {
//                foreach (var port in server.Ports)
//                {


//                    routes.Add(new ProxyRoute
//                    {

//                    });


//                    clusters.Add(new Cluster
//                    {
//                        Id = $"{server.Name}_{port.Name}_Route",
//                        Destinations =
//                        {
//                            { port.Name, new Destination { Address = $"http://{server.Address}:{port.Port}/" } }
//                        }
//                    });
//                }
//            }

//            var oldConfig = _config;
//            _config = new KubernetesProxyConfig(routes, clusters);
//            oldConfig?.SignalChange();
//        }

//        private class KubernetesProxyConfig : IProxyConfig
//        {
//            private readonly CancellationTokenSource _cts = new CancellationTokenSource();

//            public KubernetesProxyConfig(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
//            {
//                Routes = routes;
//                Clusters = clusters;
//                ChangeToken = new CancellationChangeToken(_cts.Token);
//            }

//            public IReadOnlyList<ProxyRoute> Routes { get; }

//            public IReadOnlyList<Cluster> Clusters { get; }

//            public IChangeToken ChangeToken { get; }

//            internal void SignalChange()
//            {
//                _cts.Cancel();
//            }
//        }
//    }
//}
