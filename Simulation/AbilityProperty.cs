using System.Collections.Generic;

namespace Simulation
{
    public class AbilityProperty
    {
        public float range1;
        public float angle1;
        public float range2;
        public float angle2;

        public float duration;
        public float interval;
        public float cooldown;
        public float speed;
        public List<int> items;

        public AbilityProperty Clone()
        {
            var clone = (AbilityProperty)MemberwiseClone();
            if (items != null)
            {
                clone.items = new List<int>(items);
            }
            return clone;
        }
    }
}
