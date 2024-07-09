using System;

namespace Simulation
{
    [Flags]
    public enum SpecialItemFlags
    {
        None,
        Arrow = 1,
        Potion = 2,
        Seed = 4,
        Moving = 8,
    }
    public class SpecialItem
    {
        public int typeId;
        public int entityTypeId;
        public string description;
        public ItemScript script;
        public int spawnType;
        public int impactSpawnType;
        public float launchSpeed = 10;
        public float range;
        public float range2;
        public int useItemScore;
        public int maxStackCount = int.MaxValue;
        public SpecialItemFlags flags;
    }
}
