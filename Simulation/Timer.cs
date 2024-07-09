using System;
namespace Simulation
{
    public class Timer
    {
        public double correct;
        public double interval;

        public bool AddTime(double time)
        {
            correct += time;
            bool trigger = correct > interval;
            if (trigger)
            {
                correct = Math.Min(correct - interval, interval);
            }
            return trigger;
        }

        public int NumEvent(double time)
        {
            correct += time;

            int count = (int)(correct / interval);
            correct -= count * interval;
            return count;
        }
    }
}