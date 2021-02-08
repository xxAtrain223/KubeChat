using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using KubeChat.Agones.Kubernetes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KubeChat.Agones.Services
{
    public class AgonesService : Agones.AgonesBase
    {
        private readonly IGameServerWatcher GameServerWatcher;

        public AgonesService(IGameServerWatcher gameServerWatcher)
        {
            GameServerWatcher = gameServerWatcher;
        }

        public override Task GetGameServers(Empty _empty, IServerStreamWriter<GameServerChange> responseStream, ServerCallContext context)
        {
            var requestId = Guid.NewGuid();

            void gameServerAdded(GameServerAddress gameServerAddress)
            {
                var change = new GameServerChange
                {
                    Change = GameServerChange.Types.ChangeType.Added,
                    GameServer = K8sToGRPC(gameServerAddress)
                };
                responseStream.WriteAsync(change);
            }

            void gameServerRemoved(GameServerAddress gameServerAddress)
            {
                var change = new GameServerChange
                {
                    Change = GameServerChange.Types.ChangeType.Removed,
                    GameServer = K8sToGRPC(gameServerAddress)
                };
                responseStream.WriteAsync(change);
            }

            GameServerWatcher.Register(requestId, gameServerAdded, gameServerRemoved);

            // Wait until either the client cancels or the deadline is reached
            _ = context.CancellationToken.WaitHandle.WaitOne(context.Deadline - DateTime.UtcNow);

            GameServerWatcher.Unregister(requestId);

            return Task.CompletedTask;
        }

        private static GameServerChange.Types.GameServerAddress K8sToGRPC(GameServerAddress gameServerAddress)
        {
            var gameServerAddr = new GameServerChange.Types.GameServerAddress
            {
                Name = gameServerAddress.Name,
                Address = gameServerAddress.Address
            };

            foreach (var port in gameServerAddress.Ports.Values)
            {
                gameServerAddr.Ports.Add(port.Name, new GameServerChange.Types.GameServerAddress.Types.Port
                {
                    Name = port.Name,
                    Number = port.Number
                });
            }

            return gameServerAddr;
        }
    }
}
