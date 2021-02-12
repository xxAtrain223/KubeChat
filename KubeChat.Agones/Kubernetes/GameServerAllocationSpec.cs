using k8s.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace KubeChat.Agones.Kubernetes
{
    public class GameServerAllocationSpec
    {
        [JsonProperty(PropertyName = "multiClusterSetting")]
        public MultiClusterSetting MultiClusterSetting { get; set; }

        [JsonProperty(PropertyName = "required")]
        public V1LabelSelector Required { get; set; }

        [JsonProperty(PropertyName = "preferred")]
        public IList<V1LabelSelector> Preferred { get; set; }

        [JsonProperty(PropertyName = "scheduling")]
        public GameServerSchedulingStrategey Scheduling { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public MetaPatch Metadata { get; set; }
    }
}