using System.Collections.Generic;

namespace FarmingGameServer
{
    public class ServerConfig
    {
        public string gameName;
        public HashSet<string> password;
        public float syncInterval = 1 / 10.0f;
    }
}
