using System;
using System.Linq;

namespace KubeChat.Agones.Kubernetes
{
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