using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation
{
    public abstract class BuffScript
    {
        public GameWorld World { get; set; }
        public virtual void OnAdd(Entity entity, int index)
        {

        }

        public virtual void OnRemove(Entity entity, int index)
        {

        }
    }
}
