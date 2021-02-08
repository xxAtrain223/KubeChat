using System;
using System.Linq;

namespace KubeChat.Agones.Kubernetes
{
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
}