using Simulation;
using System;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod.ItemScripts
{
    [Export(typeof(ItemScript))]
    [ExportMetadata("script", "seed_item")]
    public class SeedItem : ItemScript
    {
        public override bool Interact(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            if (Vector3.Distance(source.transform.position, position) > specialItem.range)
                return false;

            Vector3 p2 = position - source.transform.position;
            float angle = 0;
            float angle2 = 0;
            if (p2 != Vector3.Zero)
            {
                p2 = Vector3.Normalize(p2);
                angle = -MathF.Atan2(-p2.X, p2.Z);
                angle2 = -MathF.Asin(p2.Y);
            }
            var entity = World.CreateEntity(specialItem.spawnType, source.transform.position,
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle) * Quaternion.CreateFromAxisAngle(Vector3.UnitX, angle2));
            entity.projectile = new Projectile()
            {
                target = position,
                load = specialItem.impactSpawnType,
                owner = source.id,
                speed = specialItem.launchSpeed,
            };

            if (source.character.TryTakeInventoryItem(index, 1, out var item))
            {

            }

            return true;
        }

        public override AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem)
        {
            return AbilityStatus.SelectTarget;
        }

        public override AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            return AbilityStatus.Cast;
        }
    }
}
