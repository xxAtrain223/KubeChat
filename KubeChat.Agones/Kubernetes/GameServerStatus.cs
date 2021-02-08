using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KubeChat.Agones.Kubernetes
{
    public class GameServerStatus
    {
        [JsonProperty(PropertyName = "state")]
        public GameServerState State { get; set; }

        [JsonProperty(PropertyName = "ports")]
        public IList<GameServerStatusPort> Ports { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "nodeName")]
        public string NodeName { get; set; }

        [JsonProperty(PropertyName = "reservedUntil")]
        public DateTime? ReservedUntil { get; set; }

        [JsonProperty(PropertyName = "players")]
        public GameServerPlayerStatus Players { get; set; }
    }
}