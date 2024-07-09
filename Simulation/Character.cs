using System;
using System.Collections.Generic;

namespace Simulation
{
    public enum CharacterState
    {
        None,
        Working,
    }

    public enum InteractType
    {
        None = 0,
        Harvest,
        Place,
        StepIn,
        StepOut,
        Interact,
        Buy,
        Sell,
        GiveItem,
        MoveItem,
        DropItem,
        UseItem,
        BackHome,
        Chat,
        Debug,
        Remove,
        ChangeDirection

    }

    public class Character
    {
        public float facing;
        public string name;

        public CharacterState State;
        public long score;
        public bool isPlayer;
        public List<Item> inventory;
        [MessagePack.IgnoreMember]
        public int inventoryCount = 0;


        [MessagePack.IgnoreMember]
        [NonSerialized]
        public bool isLocalPlayer;

        public Item GetInventoryItem(int index)
        {
            if (inventory == null)
                return null;
            if (index < 0 || index >= inventory.Count)
                return null;
            return inventory[index];
        }

        public bool TryGetInventoryItem(int index, out Item item)
        {
            item = null;
            if (inventory == null)
                return false;
            if (index < 0 || index >= inventory.Count)
                return false;
            item = inventory[index];
            return item != null;
        }

        public bool TryTakeInventoryItem(int index, int stackCount, out Item item)
        {
            item = null;
            if (inventory == null)
                return false;
            if (index < 0 || index >= inventory.Count)
                return false;
            item = inventory[index];
            if (item.stack >= stackCount)
            {
                item.SetStack(item.stack - stackCount);
                //int cost1 = item.cost / item.stack;

                //item.stack -= stackCount;
                //item.cost -= stackCount * cost1;
            }
            if (item.stack <= 0)
            {
                inventory[index] = null;
            }

            return item != null;
        }

        public Item TakeInventoryItem(int index)
        {
            if (inventory == null)
                return null;
            if (index < 0 || index >= inventory.Count)
                return null;
            var item = inventory[index];
            inventory[index] = null;
            return item;
        }

        public Character Clone()
        {
            var clone = (Character)MemberwiseClone();
            if (inventory != null)
            {
                clone.inventory = new List<Item>(inventory);
            }
            if (clone.inventory == null && inventoryCount > 0)
            {
                clone.inventory = new List<Item>(inventoryCount);
                for (int i = 0; i < inventoryCount; i++)
                    clone.inventory.Add(null);
            }
            return clone;
        }
    }
}