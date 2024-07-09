using Simulation;
using System;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod
{
    [Export(typeof(IGameMod))]
    [ExportMetadata("id", "183B68A4-FE97-411A-8B4B-1F066B3EE177")]
    public class FarmingModApply2 : IGameMod
    {
        Random random = new Random();
        FarmingModApply farmingMod = new FarmingModApply();

        public void InitializeScripts(WorldInitializationData data)
        {
            farmingMod.InitializeScripts(data);
        }

        public void Apply(GameWorld world)
        {
            farmingMod.Apply(world);
        }

        public void CreateWorld(GameWorld world)
        {
            farmingMod.CreateWorld(world);

            for (int i = 0; i < 2000; i++)
            {
                world.CreateItem(20011, new Vector3((float)random.NextDouble() * 100 - 50, 0.4f, (float)random.NextDouble() * 100 + 10));
            }
            for (int i = 0; i < 1000; i++)
            {
                world.CreateItem(20012, new Vector3((float)random.NextDouble() * 100 - 50, 0.4f, (float)random.NextDouble() * 100 + 10));
            }

        }
    }

}