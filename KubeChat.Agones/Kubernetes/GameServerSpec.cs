using k8s.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace KubeChat.Agones.Kubernetes
{
    public class GameServerSpec
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
}