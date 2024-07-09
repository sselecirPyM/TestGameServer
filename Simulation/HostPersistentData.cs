using System.Collections.Generic;

namespace Simulation
{
    public class HostPersistentData
    {
        public Dictionary<string, Timer> timers = new Dictionary<string, Timer>();
        public Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        public Dictionary<string, int> playerName2Entity = new Dictionary<string, int>();
    }
}
