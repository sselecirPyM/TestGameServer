using System.Collections.Generic;

namespace Simulation
{
    public class Shop
    {
        public List<int> sellItems;
        public float range;

        public int GetShopItemId(int index)
        {
            if (sellItems == null || index < 0 || index >= sellItems.Count)
                return 0;
            return sellItems[index];
        }

        public Shop Clone()
        {
            Shop clone = (Shop)MemberwiseClone();
            if (sellItems != null)
                clone.sellItems = new List<int>(sellItems);
            return clone;
        }
    }
}