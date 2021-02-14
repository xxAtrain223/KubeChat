using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KubeChat.Agones.Services
{
    public class GameServerServices : IHostedService
    {
        private readonly CancellationTokenSource CancellationTokenSource;
        private readonly Agones.AgonesClient _client;
        private readonly ILogger<GameServerServices> _logger;
        private readonly Dictionary<string, K8sGameServerAddress> _gameServerAddresses;
        public IReadOnlyDictionary<string, K8sGameServerAddress> GameServerAddresses => _gameServerAddresses;

        public GameServerServices(Agones.AgonesClient client, ILogger<GameServerServices> logger)
        {
            _client = client;
            _logger = logger;
            _gameServerAddresses = new Dictionary<string, K8sGameServerAddress>();

            CancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<K8sGameServerAddress> AllocateGameServer(string requestedName)
        {
            var response = await _client.AllocateGameServerAsync(new AllocateGameServerRequest
            {
                RequestedName = requestedName
            });

            return GrpcToK8s(response);
        }

        public async Task DeleteGameServer(string gameServerName)
        {
            _ = await _client.DeleteGameServerAsync(new DeleteGameServerRequest
            {
                GameServerName = gameServerName
            });
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                var gameServerReply = _client.GetGameServers(
                    new Google.Protobuf.WellKnownTypes.Empty(),
                    deadline: DateTime.UtcNow + TimeSpan.FromHours(1),
                    cancellationToken: cancellationToken);

                await foreach (var gameServerChange in gameServerReply.ResponseStream.ReadAllAsync(cancellationToken))
                {
                    if (gameServerChange.Change == GameServerChange.Types.ChangeType.Added)
                    {
                        K8sGameServerAddress gameServerAddr = GrpcToK8s(gameServerChange.GameServer);

                        _gameServerAddresses.TryAdd(gameServerAddr.Name, gameServerAddr);
                    }
                    else if (gameServerChange.Change == GameServerChange.Types.ChangeType.Removed)
                    {
                        _gameServerAddresses.Remove(gameServerChange.GameServer.Name);
                    }
                }
            }
            catch (Exception e) when (e is RpcException re && re.StatusCode != StatusCode.DeadlineExceeded)
            {
                _logger.LogError(e, "An expcetion was thrown duing the Agones gRPC call to GetGameServers.");
            }
        }

        private static K8sGameServerAddress GrpcToK8s(GameServerAddress gameServerAddress)
        {
            return new K8sGameServerAddress
            {
                Name = gameServerAddress.Name,
                Address = gameServerAddress.Address,
                Ports = gameServerAddress.Ports.Values.ToDictionary(
                    p => p.Name,
                    p => new GameServerPort
                    {
                        Name = p.Name,
                        Number = p.Number
                    })
            };
        }

        public Task StartAsync(CancellationToken startAsyncCancellationToken)
        {
            Task.Run(async () =>
            {
                var cancellationToken = CancellationTokenSource.Token;
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ConnectAsync(cancellationToken);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }, startAsyncCancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken _)
        {
            // Cancel the gRPC calls
            CancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }
    }
}
