using Simulation;
using System;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod.ItemScripts
{
    [Export(typeof(ItemScript))]
    [ExportMetadata("script", "find_place_item")]
    public class FindPlaceItem : ItemScript
    {
        Random random = new Random();
        public override bool Interact(Entity source, int index, SpecialItem specialItem, Vector3 positionOrigin)
        {
            if (Vector3.Distance(source.transform.position, positionOrigin) > specialItem.range)
                return false;
            Vector3 position = positionOrigin;
            float radius = 0.4f;
            if (World.fastSearch.Any(position, radius, (e) =>
             {
                 return true;
             }))
            {
                for (int i = 1; i < 4; i++)
                {
                    float _max = i * 4;
                    bool find = false;
                    for (int j = 0; j < _max; j++)
                    {
                        position = positionOrigin + new Vector3(MathF.Cos(j / _max * MathF.PI * 2), 0, MathF.Sin(j / _max * MathF.PI * 2)) * i;
                        if (!World.fastSearch.Any(position, radius, (e) =>
                        {
                            return true;
                        }))
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find)
                        break;
                }
            }

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
}
