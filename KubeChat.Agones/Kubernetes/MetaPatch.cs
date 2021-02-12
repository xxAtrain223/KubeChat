using Newtonsoft.Json;
using System.Collections.Generic;

namespace KubeChat.Agones.Kubernetes
{
    public class MetaPatch
    {
        [JsonProperty(PropertyName = "labels")]
        public IDictionary<string, string> Labels { get; set; }

        [JsonProperty(PropertyName = "annotations")]
        public IDictionary<string, string> Annotations { get; set; }
    }
}