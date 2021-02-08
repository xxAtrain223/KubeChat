using Newtonsoft.Json;

namespace KubeChat.Agones.Kubernetes
{
    public class GameServerPlayersSpec
    {
        [JsonProperty(PropertyName = "initialCapacity")]
        public long InitialCapacity { get; set; }
    }
}