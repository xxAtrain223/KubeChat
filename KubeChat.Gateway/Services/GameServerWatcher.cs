using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KubeChat.Gateway.Services
{
    public class GameServerWatcher : IHostedService
    {
        private readonly Dictionary<string, GameServerAddress> _gameServerAddresses;
        public IReadOnlyDictionary<string, GameServerAddress> GameServerAddresses => _gameServerAddresses;
        private readonly CancellationTokenSource CancellationTokenSource;
        private readonly Agones.Services.Agones.AgonesClient _client;
        private readonly ILogger<GameServerWatcher> _logger;

        public GameServerWatcher(Agones.Services.Agones.AgonesClient client, ILogger<GameServerWatcher> logger)
        {
            _client = client;
            _logger = logger;
            _gameServerAddresses = new Dictionary<string, GameServerAddress>();

            CancellationTokenSource = new CancellationTokenSource();
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
                    if (gameServerChange.Change == Agones.Services.GameServerChange.Types.ChangeType.Added)
                    {
                        GameServerAddress gameServerAddr = GRPCToK8s(gameServerChange.GameServer);

                        _gameServerAddresses.TryAdd(gameServerAddr.Name, gameServerAddr);
                    }
                    else if (gameServerChange.Change == Agones.Services.GameServerChange.Types.ChangeType.Removed)
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

        private static GameServerAddress GRPCToK8s(Agones.Services.GameServerChange.Types.GameServerAddress gameServerAddress)
        {
            return new GameServerAddress
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

    public class GameServerAddress
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public IDictionary<string, GameServerPort> Ports { get; set; }
    }

    public class GameServerPort
    {
        public string Name { get; set; }
        public int Number { get; set; }
    }
}
