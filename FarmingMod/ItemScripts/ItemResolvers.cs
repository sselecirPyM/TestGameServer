using Simulation;
using System;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod.ItemScripts
{
    [Export(typeof(ItemScript))]
    [ExportMetadata("script", "bow_item")]
    public class BowItemResolver : ItemScript
    {
        public override bool Interact(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            if (Vector3.Distance(source.transform.position, position) > specialItem.range)
                return false;
            //int targetItemIndex = -1;
            //Item item = null;
            //for (int i = 0; i < source.character.inventory.Count; i++)
            //{
            //    item = source.character.inventory[i];
            //    if (item != null && World.specialItems.TryGetValue(item.typeId, out var specialItem1) && specialItem1.flags.HasFlag(SpecialItemFlags.Arrow))
            //    {
            //        targetItemIndex = i;
            //        break;
            //    }
            //}
            WorldHelper.FindFirstItem(source, World, (item, specialItem1) =>
            {
                if (specialItem1.flags.HasFlag(SpecialItemFlags.Arrow))
                    return true;
                else
                    return false;
            }, out var specialItem1, out var arrowItemIndex);
            if (source.character.TryTakeInventoryItem(arrowItemIndex, 1, out var arrow))
            {

            }
            else
            {
                return false;
            }
            Vector3 p2 = position - source.transform.position;
            float angle = 0;
            if (p2 != Vector3.Zero)
            {
                angle = -MathF.Atan2(-p2.X, p2.Z);
            }
            var entity = World.CreateEntity(specialItem.spawnType, source.transform.position, Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle));
            entity.projectile = new Projectile()
            {
                target = position,
                owner = source.id,
                speed = specialItem.launchSpeed,
            };

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

    [Export(typeof(ItemScript))]
    [ExportMetadata("script", "just_place_item")]
    public class JustPlaceItemResolver : ItemScript
    {
        Random random = new Random();
        public override bool Interact(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            if (Vector3.Distance(source.transform.position, position) > specialItem.range)
                return false;

            var entity = World.CreateEntity(specialItem.spawnType, source.transform.position,
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, random.NextSingle() * MathF.PI * 2));
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
                entity.character.score += specialItem.useItemScore;
            }

            return true;
        }

        public override AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem)
        {
            return AbilityStatus.SelectTarget;
        }

        public override AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            if (Vector3.Distance(source.transform.position, position) > specialItem.range)
                return AbilityStatus.None;
            return AbilityStatus.Cast;
        }
    }


    [Export(typeof(ItemScript))]
    [ExportMetadata("script", "sickle_item")]
    public class SickleItem : ItemScript
    {
        public override bool Interact(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            World.fastSearch.ForRange(position, 4, (e) =>
            {
                if (e.flags.HasFlag(EntityFlags.Crop))
                {
                    World.HostInteract(source, e, InteractType.Harvest, null);
                }
            });

            return true;
        }

        public override AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem)
        {
            return AbilityStatus.Cast;
        }
    }
}
