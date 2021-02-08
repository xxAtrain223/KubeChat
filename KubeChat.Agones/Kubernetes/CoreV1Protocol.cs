using System;
using System.Linq;

namespace KubeChat.Agones.Kubernetes
{
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
}