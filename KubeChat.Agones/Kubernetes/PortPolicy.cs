using System;
using System.Linq;

namespace KubeChat.Agones.Kubernetes
{
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
}