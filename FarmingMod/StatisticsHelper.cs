using Simulation;

namespace FarmingMod
{
    public static class StatisticsHelper
    {
        public static void AddSellIncome(this Entity entity,int number)
        {
            if(entity.statistics==null)
                return;
            entity.statistics.numSellIncome += number;
        }
        public static void AddBlinkDistance(this Entity entity, float distance)
        {
            if (entity.statistics == null)
                return;
            entity.statistics.numBlinkDistance += distance;
        }
    }
}
