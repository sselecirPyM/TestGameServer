using Simulation;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace FarmingMod
{
    static class WorldHelper
    {
        public static void GiveItemWithNotice(this GameWorld World, Entity entity, int itemId, int? stack = null)
        {
            if (World.itemTemplates.TryGetValue(itemId, out var template))
            {
                var item = template.Clone();
                if (stack != null)
                    item.SetStack(stack.Value);
                World.GiveItemWithNotice(entity, item);
            }
        }
        public static void GiveItemWithNotice(this GameWorld World, Entity entity, Item item)
        {
            if (entity.character != null && entity.character.isPlayer)
            {
                World.EmitEvent(new WorldEvent()
                {
                    gainItem = new GainItemEvent()
                    {
                        entityId = entity.id,
                        itemEntity = entity.id,
                        itemType = item.typeId,
                        itemCount = item.stack,
                    }
                });
            }
            World.GiveItem(entity, item);
        }

        public static void CreateWildPlant(GameWorld World, Random random, int entityTypeId)
        {
            var entity = World.CreateEntity(entityTypeId, new Vector3((float)random.NextDouble() * 100 - 50, 0, (float)random.NextDouble() * 100 - 50),
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)random.NextDouble() * MathF.PI * 2));
            if (entity?.crop == null)
                return;
            entity.crop.life = (float)(random.NextDouble() * entity.crop.maxLife / 2);
        }

        public static bool TryTakeInventoryItem(this Entity entity, int? index, out Item item)
        {
            item = null;
            if (index == null || entity.character == null)
                return false;
            item = entity.character.TakeInventoryItem(index.Value);

            return item != null;
        }

        public static float NextSingle(this Random random)
        {
            return (float)random.NextDouble();
        }

        public static bool ValidIndex<T>(this List<T> list, int index)
        {
            return index >= 0 && index < list.Count;
        }

        public static void MoveItem(this GameWorld world, Entity source, Entity entity, InteractMetaData meta)
        {
            var inventory = source.character?.inventory;
            var inventory2 = entity.character?.inventory;
            if (inventory != null && inventory2 != null && meta.i != null && meta.i2 != null)
            {
                int i1 = meta.i.Value;
                int i2 = meta.i2.Value;
                if (inventory.ValidIndex(i1) && inventory2.ValidIndex(i2))
                {
                    var item1 = inventory[i1];
                    var item2 = inventory2[i2];
                    if (item1 != null && item2 != null && item1.CanStack(item2))
                    {
                        world.StackTo(item1, item2);
                        if (item1.stack <= 0)
                            inventory[i1] = null;
                    }
                    else
                    {
                        (inventory2[i2], inventory[i1]) = (inventory[i1], inventory2[i2]);
                    }
                    world.HostSyncEntity(source);
                    world.HostSyncEntity(entity);
                }
            }
        }

        public static void ShowMenuLocal(this GameWorld world, Entity source, Entity entity, ShowMenuType type, float range)
        {
            world.EmitEventLocal(new WorldEvent()
            {
                showMenu = new ShowMenuEvent()
                {
                    entityId = source.id,
                    targetId = entity.id,
                    range = range,
                    type = type
                }
            });
        }

        public static bool FindFirstItem(Entity entity, GameWorld world, Func<Item, SpecialItem, bool> filter, out SpecialItem specialItem, out int index)
        {
            index = -1;
            specialItem = null;
            var inventory = entity.character?.inventory;
            if (inventory == null)
            {
                return false;
            }
            for (int i = 0; i < inventory.Count; i++)
            {
                var item = inventory[i];
                if (item != null && world.specialItems.TryGetValue(item.typeId, out specialItem) && filter(item, specialItem))
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        public static void EntitySetScript(this GameWorld world, Entity entity, string script)
        {
            world.scripts.TryGetValue(script, out entity.script);
        }
    }
}
