namespace Simulation
{
    public class Buff
    {
        public string name;
        public string description;
        public int typeId;
        public int level;

        public Buff Clone()
        {
            return (Buff)MemberwiseClone();
        }
    }
}