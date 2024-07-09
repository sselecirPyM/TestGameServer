namespace Simulation
{
    public class Crop
    {
        public string harvestSound;
        public float water;
        public float maxWater;
        public float nutrition;
        public float maxNutrition;
        public float growthThreshold;

        public int harvestTimes;
        public int maxHarvestTimes;

        public int growthId;

        public int lootId;
        public int productCount = 1;
        public int randomProductCount;

        public int harvestScore;

        public float life;
        public float maxLife;

        public Crop Clone()
        {
            return (Crop)MemberwiseClone();
        }
    }
}