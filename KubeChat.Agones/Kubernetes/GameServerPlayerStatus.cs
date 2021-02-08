using Newtonsoft.Json;
using System.Collections.Generic;

namespace KubeChat.Agones.Kubernetes
{
    public class GameServerPlayerStatus
    {
        [JsonProperty(PropertyName = "count")]
        public long Count { get; set; }

        [JsonProperty(PropertyName = "capacity")]
        public long Capacity { get; set; }

        [JsonProperty(PropertyName = "ids")]
        public IList<string> Ids { get; set; }
    }
}