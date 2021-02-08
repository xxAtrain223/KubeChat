using k8s;
using k8s.Models;
using Newtonsoft.Json;

namespace KubeChat.Agones.Kubernetes
{
    [KubernetesEntity(Group = "agones.dev", ApiVersion = "v1", Kind = "GameServer", PluralName = "gameservers")]
    public class GameServer : IKubernetesObject<V1ObjectMeta>, ISpec<GameServerSpec>
    {
        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }

        [JsonProperty(PropertyName = "spec")]
        public GameServerSpec Spec { get; set; }

        [JsonProperty(PropertyName = "status")]
        public GameServerStatus Status { get; set; }
    }
}