using Simulation;
using System;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod
{
    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "daylight")]
    public class DaylightResolver : EntityScript
    {
        public override void Update(Entity entity)
        {
            var angle = World.persistentData.timeOfDay / (24 * 60) * 2 * MathF.PI;
            entity.transform.rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle) * Quaternion.CreateFromAxisAngle(Vector3.UnitX, -60.0f / 180.0f * MathF.PI);
            entity.updateModel = true;
        }
        public override void ClientUpdate(Entity entity)
        {
            Update(entity);
        }
    }
}
