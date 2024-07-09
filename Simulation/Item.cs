namespace Simulation
{
    public class Item
    {
        public string name;
        public string icon;
        public int typeId;
        public int stack;
        public int cost;

        public bool CanStack(Item other)
        {
            return typeId == other.typeId &&
            icon == other.icon &&
            name == other.name &&
            stack != 0 &&
            other.stack != 0;
        }

        public void StackTo(Item target, int count)
        {
            int cost1 = cost / stack * count;
            target.stack += count;
            target.cost += cost1;

            stack -= count;
            cost -= cost1;
        }

        public void SetStack(int count)
        {
            cost = cost / stack * count;
            stack = count;
        }

        public Item Clone()
        {
            return (Item)MemberwiseClone();
        }
    }
}