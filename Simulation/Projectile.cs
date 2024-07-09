using System.Collections.Generic;
using System.Numerics;

namespace Simulation
{
    public class Projectile
    {
        public Vector3 target;
        public int owner;
        public int load;
        public float speed;

        public HashSet<int> hits = new HashSet<int>();

        public Projectile Clone()
        {
            return (Projectile)MemberwiseClone();
        }
    }
}
