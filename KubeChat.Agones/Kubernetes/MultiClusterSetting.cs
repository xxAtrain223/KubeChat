using k8s.Models;
using Newtonsoft.Json;

namespace KubeChat.Agones.Kubernetes
{
    public class MultiClusterSetting
    {
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "policySelector")]
        public V1LabelSelector PolicySelector { get; set; }
    }
}