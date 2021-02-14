using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using k8s;
using KubeChat.Agones.Kubernetes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KubeChat.Agones
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

            void gameServerAdded(K8sGameServerAddress gameServerAddress)
            {
                var change = new GameServerChange
                {
                    Change = GameServerChange.Types.ChangeType.Added,
                    GameServer = K8sToGrpc(gameServerAddress)
                };
                responseStream.WriteAsync(change);
            }

            void gameServerRemoved(K8sGameServerAddress gameServerAddress)
            {
                var change = new GameServerChange
                {
                    Change = GameServerChange.Types.ChangeType.Removed,
                    GameServer = K8sToGrpc(gameServerAddress)
                };
                responseStream.WriteAsync(change);
            }

            GameServerWatcher.Register(requestId, gameServerAdded, gameServerRemoved);

            // Wait until either the client cancels or the deadline is reached
            _ = context.CancellationToken.WaitHandle.WaitOne(context.Deadline - DateTime.UtcNow);

            GameServerWatcher.Unregister(requestId);

            return Task.CompletedTask;
        }

        private static GameServerAddress K8sToGrpc(K8sGameServerAddress gameServerAddress)
        {
            var gameServerAddr = new GameServerAddress
            {
                Name = gameServerAddress.Name,
                Address = gameServerAddress.Address
            };

            foreach (var port in gameServerAddress.Ports.Values)
            {
                gameServerAddr.Ports.Add(port.Name, new GameServerAddress.Types.Port
                {
                    Name = port.Name,
                    Number = port.Number
                });
            }

            return gameServerAddr;
        }

        public override async Task<GameServerAddress> AllocateGameServer(AllocateGameServerRequest request, ServerCallContext context)
        {
            var kubernetesConfig = KubernetesClientConfiguration.InClusterConfig();
            var kubernetesClient = new k8s.Kubernetes(kubernetesConfig);

            var allocation = new GameServerAllocation
            {
                Spec = new GameServerAllocationSpec
                {
                    Required = new k8s.Models.V1LabelSelector
                    {
                        MatchLabels = new Dictionary<string, string>
                        {
                            { "agones.dev/fleet", "server-fleet" }
                        }
                    }
                }
            };

            allocation = await kubernetesClient.CreateClusterCustomObjectAsync(allocation, "allocation.agones.dev", "v1", "gameserverallocations") as GameServerAllocation;

            if (allocation.Status.State == GameServerAllocationState.Allocated)
            {
                var gameServerAddress = new GameServerAddress
                {
                    Name = allocation.Status.GameServerName,
                    Address = allocation.Status.Address
                };

                foreach (var port in allocation.Status.Ports)
                {
                    gameServerAddress.Ports.Add(port.Name, new GameServerAddress.Types.Port
                    {
                        Name = port.Name,
                        Number = port.Number
                    });
                }

                return gameServerAddress;
            }
            else
            {
                throw new RpcException(new Status(StatusCode.ResourceExhausted, $"Error allocating GameServer, State: {allocation.Status.State}"));
            }
        }

        public override async Task<Empty> DeleteGameServer(DeleteGameServerRequest request, ServerCallContext context)
        {
            var kubernetesConfig = KubernetesClientConfiguration.InClusterConfig();
            var kubernetesClient = new k8s.Kubernetes(kubernetesConfig);

            await kubernetesClient.DeleteClusterCustomObjectAsync("agones.dev", "v1", "gameservers", request.GameServerName);

            return new Empty();
        }
    }
}
