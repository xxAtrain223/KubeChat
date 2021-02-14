using System.Collections.Generic;

namespace KubeChat.Agones
{
    public class K8sGameServerAddress
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public IDictionary<string, GameServerPort> Ports { get; set; }
    }
}
