using Simulation;
using System;

namespace FarmingMod
{
    public static class EntityHelper
    {
        public static int ItemCount(this Entity entity, int itemTypeId)
        {
            if (entity.character == null || entity.character.inventory == null)
                return 0;
            int itemCount = 0;
            foreach (var item in entity.character.inventory)
            {
                if (item == null)
                    continue;
                if (item.typeId == itemTypeId)
                {
                    itemCount += item.stack;
                }
            }
            return itemCount;
        }

        public static void RemoveItems(this Entity entity, int itemTypeId, int count)
        {
            if (entity.character == null || entity.character.inventory == null)
                return;
            var inventory = entity.character.inventory;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (count <= 0)
                    break;
                var item = inventory[i];
                if (item == null || item.typeId != itemTypeId)
                    continue;

                int count1 = Math.Min(count, item.stack);
                count -= count1;
                entity.character.TryTakeInventoryItem(i, count1, out item);
            }
        }
    }
}
