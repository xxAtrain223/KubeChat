using System;
using System.Linq;

namespace KubeChat.Agones.Kubernetes
{
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
}