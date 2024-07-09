using System.Collections.Generic;

namespace Simulation
{
    public class WorldPersistentData
    {
        public Dictionary<string, Timer> timers = new Dictionary<string, Timer>();
        public float timeOfDay;
        public int difficulty;
    }
}
