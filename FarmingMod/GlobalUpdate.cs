using Simulation;
using System;

namespace FarmingMod
{
    public class GlobalUpdate : GlobalUpdateScript
    {
        float oneDayTime = 24 * 60;
        Timer timer = new Timer() { interval = 14 };
        Random random = new Random();
        int[] entityTypeList = new int[] { 2000, 2001 };

        float timeMultiple = 8;

        public override void Update()
        {
            ref var tod = ref World.persistentData.timeOfDay;
            tod += World.fixedDeltaTime * timeMultiple;
            if (tod > 24 * 60)
            {
                tod -= oneDayTime;
            }

            if (timer.AddTime(World.fixedDeltaTime) && World.IsDay())
            {
                //var entity = World.CreateEntity(entityTypeList[random.Next(0, entityTypeList.Length)], new Vector3((float)random.NextDouble() * 100 - 50, 0, (float)random.NextDouble() * 100 - 50),
                //    Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)random.NextDouble() * MathF.PI * 2));
                //entity.crop.life = (float)(random.NextDouble() * entity.crop.maxLife / 2);
                WorldHelper.CreateWildPlant(World, random, entityTypeList[random.Next(0, entityTypeList.Length)]);
            }

        }

        public override void ClientUpdate()
        {
            ref var tod = ref World.persistentData.timeOfDay;
            tod += World.fixedDeltaTime * timeMultiple;
            if (tod > 24 * 60)
            {
                tod -= oneDayTime;
            }
        }
    }
}
