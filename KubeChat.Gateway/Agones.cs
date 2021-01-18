using k8s;
using k8s.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KubeChat.Gateway.Agones
{
    [KubernetesEntity(Group = "agones.dev", ApiVersion = "v1", Kind = "GameServer", PluralName = "gameservers")]
    public class GameServer : IKubernetesObject<V1ObjectMeta>, ISpec<GameServerSpec> //, IValidate
    {
        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; } // agones.dev/v1

        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; } // GameServer

        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }

        [JsonProperty(PropertyName = "spec")]
        public GameServerSpec Spec { get; set; }

        [JsonProperty(PropertyName = "status")]
        public GameServerStatus Status { get; set; }

        //public void Validate()
        //{
        //    Spec?.Validate();
        //}
    }

    public class GameServerSpec // : IValidate
    {
        [JsonProperty(PropertyName = "container")]
        public string Container { get; set; }

        [JsonProperty(PropertyName = "ports")]
        public IList<GameServerPort> Ports { get; set; }

        [JsonProperty(PropertyName = "health")]
        public GameServerHealth Health { get; set; }

        [JsonProperty(PropertyName = "scheduling")]
        public GameServerSchedulingStrategey Scheduling { get; set; }

        [JsonProperty(PropertyName = "sdkServer")]
        public GameServerSdkServer SdkServer { get; set; }

        [JsonProperty(PropertyName = "template")]
        public V1PodTemplateSpec Template { get; set; }

        [JsonProperty(PropertyName = "players")]
        public GameServerPlayersSpec Players { get; set; }
    }

    public class GameServerPort
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "portPolicy")]
        public PortPolicy PortPolicy { get; set; }

        [JsonProperty(PropertyName = "container")]
        public string Container { get; set; }

        [JsonProperty(PropertyName = "containerPort")]
        public int ContainerPort { get; set; }

        [JsonProperty(PropertyName = "hostPort")]
        public int HostPort { get; set; }

        [JsonProperty(PropertyName = "protocol")]
        public CoreV1Protocol Protocol { get; set; }
    }

    public class GameServerHealth
    {
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }

        [JsonProperty(PropertyName = "periodSeconds")]
        public int PeriodSeconds { get; set; }

        [JsonProperty(PropertyName = "failureThreshold")]
        public int FailureThreshold { get; set; }

        [JsonProperty(PropertyName = "initialDelaySeconds")]
        public int InitialDelaySeconds { get; set; }
    }

    public class GameServerSdkServer
    {
        [JsonProperty(PropertyName = "logLevel")]
        public SdkServerLogLevel LogLevel { get; set; }

        [JsonProperty(PropertyName = "grpcPort")]
        public int GRPCPort { get; set; }

        [JsonProperty(PropertyName = "httpPort")]
        public int HTTPPort { get; set; }
    }

    public class GameServerPlayersSpec
    {
        [JsonProperty(PropertyName = "initialCapacity")]
        public long InitialCapacity { get; set; }
    }

    public class GameServerStatus
    {
        [JsonProperty(PropertyName = "state")]
        public GameServerState State { get; set; }

        [JsonProperty(PropertyName = "ports")]
        public IList<GameServerStatusPort> Ports { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "nodeName")]
        public string NodeName { get; set; }

        [JsonProperty(PropertyName = "reservedUntil")]
        public DateTime? ReservedUntil { get; set; }

        [JsonProperty(PropertyName = "players")]
        public GameServerPlayerStatus Players { get; set; }
    }

    public class GameServerStatusPort
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "port")]
        public int Port { get; set; }
    }

    public class GameServerPlayerStatus
    {
        [JsonProperty(PropertyName = "count")]
        public long Count { get; set; }

        [JsonProperty(PropertyName = "capacity")]
        public long Capacity { get; set; }

        [JsonProperty(PropertyName = "ids")]
        public IList<string> Ids { get; set; }
    }

    public class GameServerList : IKubernetesObject<V1ListMeta>, IMetadata<V1ListMeta>, IItems<GameServer> //, IValidate
    {
        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public V1ListMeta Metadata { get; set; }

        [JsonProperty(PropertyName = "items")]
        public IList<GameServer> Items { get; set; }

        //public void Validate()
        //{
        //    if (Items == null)
        //    {
        //        throw new ValidationException(ValidationRules.CannotBeNull, nameof(Items));
        //    }
        //    if (Items != null)
        //    {
        //        foreach (var element in Items)
        //        {
        //            if (element != null)
        //            {
        //                element.Validate();
        //            }
        //        }
        //    }
        //}
    }

    public struct PortPolicy
    {
        public const string Dynamic = "Dynamic";
        public const string Static = "Static";
        public const string Passthrough = "Passthrough";

        private static readonly string[] Policies = { Dynamic, Static, Passthrough };

        private readonly string _policy;

        public PortPolicy(string policy)
        {
            if (string.IsNullOrEmpty(policy))
            {
                _policy = Dynamic;
            }
            else if (Policies.Contains(policy))
            {
                _policy = policy;
            }
            else
            {
                throw new ArgumentException($"Policy is not {string.Join(", ", Policies)}", nameof(policy));
            }
        }

        public override string ToString()
        {
            return _policy;
        }

        public static implicit operator PortPolicy(string value)
        {
            return new PortPolicy(value);
        }

        public static implicit operator string(PortPolicy value)
        {
            return value._policy;
        }
    }

    public struct CoreV1Protocol
    {
        public const string TCP = "TCP";
        public const string UDP = "UDP";
        public const string TCPUDP = "TCPUDP";

        private static readonly string[] Protocols = { TCP, UDP, TCPUDP };

        private readonly string _protocol;

        public CoreV1Protocol(string protocol)
        {
            if (string.IsNullOrEmpty(protocol))
            {
                _protocol = UDP;
            }
            else if (Protocols.Contains(protocol))
            {
                _protocol = protocol;
            }
            else
            {
                throw new ArgumentException($"Protocol is not {string.Join(", ", Protocols)}", nameof(protocol));
            }
        }

        public override string ToString()
        {
            return _protocol;
        }

        public static implicit operator CoreV1Protocol(string value)
        {
            return new CoreV1Protocol(value);
        }

        public static implicit operator string(CoreV1Protocol value)
        {
            return value._protocol;
        }
    }

    public struct GameServerSchedulingStrategey
    {
        public const string Packed = "Packed";
        public const string Distributed = "Distributed";

        private static readonly string[] Strategies = { Packed, Distributed };

        private readonly string _strategy;

        public GameServerSchedulingStrategey(string strategy)
        {
            if (string.IsNullOrEmpty(strategy))
            {
                _strategy = Packed;
            }
            else if (Strategies.Contains(strategy))
            {
                _strategy = strategy;
            }
            else
            {
                throw new ArgumentException($"Strategy is not {string.Join(", ", Strategies)}", nameof(strategy));
            }
        }

        public override string ToString()
        {
            return _strategy;
        }

        public static implicit operator GameServerSchedulingStrategey(string value)
        {
            return new GameServerSchedulingStrategey(value);
        }

        public static implicit operator string(GameServerSchedulingStrategey value)
        {
            return value._strategy;
        }
    }

    public struct SdkServerLogLevel
    {
        public const string Info = "Info";
        public const string Debug = "Debug";
        public const string Error = "Error";

        private static readonly string[] LogLevels = { Info, Debug, Error };

        private readonly string _logLevel;

        public SdkServerLogLevel(string logLevel)
        {
            if (string.IsNullOrEmpty(logLevel))
            {
                _logLevel = Info;
            }
            else if (LogLevels.Contains(logLevel))
            {
                _logLevel = logLevel;
            }
            else
            {
                throw new ArgumentException($"LogLevel is not {string.Join(", ", LogLevels)}", nameof(logLevel));
            }
        }

        public override string ToString()
        {
            return _logLevel;
        }

        public static implicit operator SdkServerLogLevel(string value)
        {
            return new SdkServerLogLevel(value);
        }

        public static implicit operator string(SdkServerLogLevel value)
        {
            return value._logLevel;
        }
    }

    public struct GameServerState
    {
        public const string PortAllocation = "PortAllocation";
        public const string Creating = "Creating";
        public const string Starting = "Starting";
        public const string Scheduled = "Scheduled";
        public const string RequestReady = "RequestReady";
        public const string Ready = "Ready";
        public const string Shutdown = "Shutdown";
        public const string Error = "Error";
        public const string Unhealthy = "Unhealthy";
        public const string Reserved = "Reserved";
        public const string Allocated = "Allocated";

        private static readonly string[] States = { PortAllocation, Creating, Starting, Scheduled, RequestReady, Ready, Shutdown, Error, Unhealthy, Reserved, Allocated };

        private readonly string _state;

        public GameServerState(string state)
        {
            if (string.IsNullOrEmpty(state))
            {
                _state = "";
            }
            else if (States.Contains(state))
            {
                _state = state;
            }
            else
            {
                throw new ArgumentException($"State is not {string.Join(", ", States)}", nameof(state));
            }
        }

        public override string ToString()
        {
            return _state;
        }

        public static implicit operator GameServerState(string value)
        {
            return new GameServerState(value);
        }

        public static implicit operator string(GameServerState value)
        {
            return value._state;
        }
    }
}