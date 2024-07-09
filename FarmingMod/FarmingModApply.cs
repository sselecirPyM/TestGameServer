using Simulation;
using System;
using System.Numerics;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Collections.Generic;

namespace FarmingMod
{
    [Export(typeof(IGameMod))]
    [ExportMetadata("id", "F7FE3AB6-DC78-4C0A-B66F-0773C63035E2")]
    public class FarmingModApply : IGameMod
    {
        public void InitializeScripts(WorldInitializationData data)
        {
            var cat = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(new AggregateCatalog(cat));

            var entityScripts = container.GetExports<EntityScript, IDictionary<string, object>>();
            foreach (var e in entityScripts)
            {
                if (e.Metadata.TryGetValue("script", out var v) && v != null)
                {
                    data.EntityScripts[v.ToString()] = e.Value;
                }
            }

            var itemScripts = container.GetExports<ItemScript, IDictionary<string, object>>();
            foreach (var e in itemScripts)
            {
                if (e.Metadata.TryGetValue("script", out var v) && v != null)
                {
                    data.ItemScripts[v.ToString()] = e.Value;
                }
            }
        }

        Random random = new Random();
        public void Apply(GameWorld world)
        {
            world.HostOnPlayerJoin += FarmingGamePlayerProcess.PlayerJoin;
            world.HostOnPlayerLeave += FarmingGamePlayerProcess.PlayerLeave;
            world.globalUpdateScript = new GlobalUpdate() { World = world };
            world.persistentData.timeOfDay = 6 * 60;

            //var cat = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            //var container = new CompositionContainer(new AggregateCatalog(cat));

            //var entityScripts = container.GetExports<EntityScript, IDictionary<string, object>>();
            //foreach (var e in entityScripts)
            //{
            //    if (e.Metadata.TryGetValue("script", out var v) && v != null)
            //    {
            //        world.AddScript(v.ToString(), e.Value);
            //    }
            //}

            //var itemScripts = container.GetExports<ItemScript, IDictionary<string, object>>();
            //foreach (var e in itemScripts)
            //{
            //    if (e.Metadata.TryGetValue("script", out var v) && v != null)
            //    {
            //        world.AddScript(v.ToString(), e.Value);
            //    }
            //}
        }

        public void CreateWorld(GameWorld world)
        {
            world.CreateEntity(900, new Vector3(0, 0, 0), Quaternion.Identity);
            for (int i = 0; i < 50; i++)
            {
                WorldHelper.CreateWildPlant(world, random, 2000);
            }
            for (int i = 0; i < 50; i++)
            {
                WorldHelper.CreateWildPlant(world, random, 2001);
            }
            for (int i = 0; i < 5; i++)
            {
                world.CreateEntity(4020, new Vector3((float)random.NextDouble() * 100 - 50, 0, (float)random.NextDouble() * 100 - 50),
                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)random.NextDouble() * MathF.PI * 2), Vector3.One * ((float)random.NextDouble() * 0.5f + 2.5f));
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    world.CreateEntity(4020, new Vector3((float)random.NextDouble() * 1 + i * 5 + 100, 0, (float)random.NextDouble() * 1 - 30 + j * 8),
                        Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)random.NextDouble() * MathF.PI * 2), Vector3.One * ((float)random.NextDouble() * 0.5f + 2.5f));
                }
            }

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 5; j++)
                    for (int k = 0; k < 4; k++)
                        world.CreateEntity(5000, new Vector3((i - 4) * 30 + k * 3, 0, -j * 3), Quaternion.Identity);
            }

            //for (int i = 0; i < 5; i++)
            //{
            //    world.CreateEntity(10010, new Vector3(i * 20 - 50, 0, 10), Quaternion.Identity);
            //}

            for (int i = -1; i < 2; i++)
            {
                world.CreateEntity(9000, new Vector3(i * 80, 0, 20), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI));
                world.CreateEntity(9001, new Vector3(i * 80 + 5, 0, -20), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI));
            }
        }
    }

}