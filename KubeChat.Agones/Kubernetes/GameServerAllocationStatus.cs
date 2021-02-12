using Newtonsoft.Json;
using System.Collections.Generic;

namespace KubeChat.Agones.Kubernetes
{
    public class GameServerAllocationStatus
    {
        [JsonProperty(PropertyName = "state")]
        public GameServerAllocationState State { get; set; }

        [JsonProperty(PropertyName = "gameServerName")]
        public string GameServerName { get; set; }

        [JsonProperty(PropertyName = "ports")]
        public IList<GameServerStatusPort> Ports { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "nodeName")]
        public string NodeName { get; set; }
    }
}