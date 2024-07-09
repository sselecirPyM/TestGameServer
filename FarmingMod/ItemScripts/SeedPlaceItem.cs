using Simulation;
using System;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod.ItemScripts
{
    [Export(typeof(ItemScript))]
    [ExportMetadata("script", "seed_place_item")]
    public class SeedPlaceItem : ItemScript
    {
        Random random = new Random();
        public override bool Interact(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            if (Vector3.Distance(source.transform.position, position) > specialItem.range)
                return false;
            Entity t = null;
            World.fastSearch.Nearest(position, 4, (e) => CanFarming(World, e), (e) =>
            {
                t = e;
            });
            if (t == null)
                return false;

            var entity = World.CreateEntity(specialItem.spawnType, source.transform.position,
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, (random.NextSingle() - 0.5f) * MathF.PI * 0.1f));
            World.EntitySetScript(entity, "just_place");
            entity.projectile = new Projectile()
            {
                target = t.transform.position + t.farmingField.attachPoint,
                load = specialItem.impactSpawnType,
                owner = source.id,
                speed = specialItem.launchSpeed,
            };
            t.farmingField.cropId = entity.id;
            World.HostSyncEntity(t);

            if (source.character.TryTakeInventoryItem(index, 1, out var item))
            {
                source.character.score += specialItem.useItemScore;
            }

            return true;
        }

        public override AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem)
        {
            if (World.fastSearch.Any(source.transform.position, 4, (e) => CanFarming(World, e)))
            {
                return AbilityStatus.Cast;
            }
            else
            {
                return AbilityStatus.SelectTarget;
            }
        }

        public override AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            if (Vector3.Distance(source.transform.position, position) > specialItem.range)
                return AbilityStatus.None;
            return AbilityStatus.Cast;
        }

        static bool CanFarming(GameWorld World, Entity e)
        {
            return e.farmingField != null && !World.id2Entity.ContainsKey(e.farmingField.cropId);
        }
    }
}
