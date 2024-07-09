using System.Numerics;

namespace Simulation
{
    public abstract class ItemScript
    {
        public GameWorld World { get; set; }

        public virtual void FrameBegin()
        {

        }

        public virtual bool Interact(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            return false;
        }

        public virtual AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem)
        {
            return AbilityStatus.None;
        }

        public virtual AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            return AbilityStatus.None;
        }

    }
}
