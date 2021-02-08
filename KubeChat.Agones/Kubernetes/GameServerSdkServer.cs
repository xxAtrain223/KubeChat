using Newtonsoft.Json;

namespace KubeChat.Agones.Kubernetes
{
    public class GameServerSdkServer
    {
        [JsonProperty(PropertyName = "logLevel")]
        public SdkServerLogLevel LogLevel { get; set; }

        [JsonProperty(PropertyName = "grpcPort")]
        public int GRPCPort { get; set; }

        [JsonProperty(PropertyName = "httpPort")]
        public int HTTPPort { get; set; }
    }
}