using System.Collections.Generic;
using System.Numerics;

namespace Simulation
{
    public interface IGameMod
    {
        public void InitializeScripts(WorldInitializationData worldInitializationData);

        public virtual void Apply(GameWorld world)
        {

        }

        public virtual void CreateWorld(GameWorld world)
        {

        }

        public virtual void CreateBlock64(GameWorld world, Vector2 position)
        {

        }
    }

    public class WorldInitializationData
    {
        public Dictionary<string, EntityScript> EntityScripts { get; } = new Dictionary<string, EntityScript>();
        public Dictionary<string, ItemScript> ItemScripts { get; } = new Dictionary<string, ItemScript>();

        public void Apply(GameWorld world)
        {
            foreach(var  script in EntityScripts)
            {
                world.AddScript(script.Key, script.Value);
            }
            foreach (var script in ItemScripts)
            {
                world.AddScript(script.Key, script.Value);
            }
        }
    }
}
