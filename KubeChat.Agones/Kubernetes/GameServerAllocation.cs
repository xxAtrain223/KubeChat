using k8s;
using k8s.Models;
using Newtonsoft.Json;

namespace KubeChat.Agones.Kubernetes
{
    [KubernetesEntity(Group = "allocation.agones.dev", ApiVersion = "v1", Kind = "GameServerAllocation")]
    public class GameServerAllocation : IKubernetesObject<V1ObjectMeta>, ISpec<GameServerAllocationSpec>
    {
        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; } = "allocation.agones.dev/v1";

        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; } = "GameServerAllocation";

        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }

        [JsonProperty(PropertyName = "spec")]
        public GameServerAllocationSpec Spec { get; set; }

        [JsonProperty(PropertyName = "status")]
        public GameServerAllocationStatus Status { get; set; }
    }
}
