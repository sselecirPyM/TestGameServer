using System.Numerics;

namespace Simulation
{
    public class FarmingField
    {
        public int cropId;
        public Vector3 attachPoint;
        public float water;
        public float maxWater;
        public float waterDeltaDay;
        public float waterDeltaNight;

        public FarmingField Clone()
        {
            return (FarmingField)MemberwiseClone();
        }
    }
}
