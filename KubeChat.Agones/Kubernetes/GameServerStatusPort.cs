using Newtonsoft.Json;

namespace KubeChat.Agones.Kubernetes
{
    public class GameServerStatusPort
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "port")]
        public int Number { get; set; }
    }
}