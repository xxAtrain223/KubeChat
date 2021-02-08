using Newtonsoft.Json;

namespace KubeChat.Agones.Kubernetes
{
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
}