namespace Simulation
{
    public class Ability
    {
        public string name;
        public string description;
        public string icon;
        public int typeId;
        public string script;
        public float range;
        public float power;
        public float cost;
        public float cooldown;

        public Ability Clone()
        {
            return (Ability)MemberwiseClone();
        }
    }
}