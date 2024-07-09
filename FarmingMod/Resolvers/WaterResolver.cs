using Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod
{
    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "water")]
    public class WaterResolver : EntityScript
    {
        Timer timer = new Timer() { interval = 1 };
        bool update = false;
        public override void FrameBegin()
        {
            update = timer.AddTime(World.fixedDeltaTime);
        }

        List<Entity> list = new List<Entity>();
        public override void Update(Entity entity)
        {
            if (!update || entity.crop == null)
                return;
            World.fastSearch.ForRange(entity.transform.position, 4, (e) =>
            {
                if ((e.crop != null && e.crop.maxWater - e.crop.water > 0.5f) || e.farmingField != null)
                {
                    list.Add(e);
                }
            });
            if (list.Count > 0)
            {
                float rate = 0.5f;
                float totalWaterEmit = entity.crop.water * rate;
                float waterPerEntity = totalWaterEmit / list.Count;
                entity.crop.water -= totalWaterEmit;
                foreach (Entity e in list)
                {
                    if (e.crop != null)
                    {
                        float waterAdded = Math.Min(waterPerEntity, Math.Max(e.crop.maxWater - e.crop.water, 0));
                        e.crop.water += waterAdded;
                        totalWaterEmit -= waterAdded;
                    }
                    else if (e.farmingField != null)
                    {
                        float waterAdded = Math.Min(waterPerEntity, Math.Max(e.farmingField.maxWater - e.farmingField.water, 0));
                        e.farmingField.water += waterAdded;
                        totalWaterEmit -= waterAdded;
                    }
                }
                entity.crop.water += totalWaterEmit;
            }

            list.Clear();
            entity.crop.water -= 5;
            World.HostSyncEntity(entity);
            if (entity.crop.water <= 0.1f)
            {
                World.DestroyEntity(entity);
            }
            else
            {
                entity.transform.scale = new Vector3(MathF.Pow(entity.crop.water / MathF.Max(entity.crop.maxWater, 1), 1 / 3.0f));
            }
        }
    }
}
