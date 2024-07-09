using Simulation;
using System;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod.ItemScripts
{
    [Export(typeof(ItemScript))]
    [ExportMetadata("script", "place_facing_item")]
    public class PlaceFacingItem : ItemScript
    {
        Random random = new Random();
        public override bool Interact(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            var distance = Vector3.Distance(source.transform.position, position);
            if (distance > specialItem.range || distance < 1e-3f)
                return false;
            var ePosition = source.transform.position;

            float y1 = position.Z - ePosition.Z;
            float x1 = position.X - ePosition.X;
            if (y1 < 1e-5f && y1 > -1e-5f && x1 < 1e-5f && x1 > -1e-5f)
            {
                x1 = 1e-5f;
            }
            var entity = World.CreateEntity(specialItem.spawnType, source.transform.position,
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.Atan2(x1, y1)));
            World.EntitySetScript(entity, "just_place");
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
            var distance = Vector3.Distance(source.transform.position, position);
            if (distance > specialItem.range || distance < 1e-3f)
                return AbilityStatus.None;
            return AbilityStatus.Cast;
        }
    }
}
