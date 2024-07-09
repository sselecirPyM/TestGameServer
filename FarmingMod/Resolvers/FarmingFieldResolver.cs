using Simulation;
using System;
using System.ComponentModel.Composition;

namespace FarmingMod
{
    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "farming_field")]
    public class FarmingFieldResolver : EntityScript
    {
        Timer timer = new Timer() { interval = 1.0f };
        bool trigger = false;
        public override void FrameBegin()
        {
            trigger = timer.AddTime(World.fixedDeltaTime);
        }
        public override void Update(Entity entity)
        {
            if (trigger && entity.farmingField != null)
            {
                var farmingField = entity.farmingField;
                if (World.TryGetEntity(farmingField.cropId, out var cropEntity) && cropEntity.crop != null)
                {
                    float giveWater = Math.Max(Math.Min(farmingField.water, cropEntity.crop.maxWater - cropEntity.crop.water), 0);
                    farmingField.water -= giveWater;
                    cropEntity.crop.water += giveWater;
                }
                if (World.IsDay())
                    farmingField.water = Math.Clamp(farmingField.water + farmingField.waterDeltaDay, 0, farmingField.maxWater);
                else
                    farmingField.water = Math.Clamp(farmingField.water + farmingField.waterDeltaNight, 0, farmingField.maxWater);

            }
        }
    }
}
