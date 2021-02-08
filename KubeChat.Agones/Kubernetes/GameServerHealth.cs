using Newtonsoft.Json;

namespace KubeChat.Agones.Kubernetes
{
    public class GameServerHealth
    {
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }

        [JsonProperty(PropertyName = "periodSeconds")]
        public int PeriodSeconds { get; set; }

        [JsonProperty(PropertyName = "failureThreshold")]
        public int FailureThreshold { get; set; }

        [JsonProperty(PropertyName = "initialDelaySeconds")]
        public int InitialDelaySeconds { get; set; }
    }
}