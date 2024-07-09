using System;
using System.Numerics;

namespace Simulation
{
    public abstract class EntityScript
    {
        public GameWorld World { get; set; }
        public virtual void FrameBegin()
        {

        }

        public virtual void Update(Entity entity)
        {

        }

        public virtual void ClientUpdate(Entity entity)
        {

        }

        public virtual void OnDestroy(Entity entity)
        {

        }

        public virtual void ClientOnDestroy(Entity entity)
        {

        }

        public virtual void Interact(Entity source, Vector3 position, InteractType interactType, InteractMetaData meta)
        {

        }

        public virtual void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {

        }

        public virtual bool ClientInteract(Entity source, Vector3 position, InteractType interactType, InteractMetaData meta)
        {
            return false;
        }

        public virtual bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            return false;
        }

        //public virtual void ParallelUpdate(Entity entity)
        //{
        //}
    }

}