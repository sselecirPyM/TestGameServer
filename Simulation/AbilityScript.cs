using System.Numerics;

namespace Simulation
{
    public enum AbilityStatus
    {
        None,
        Cast,
        SelectTarget,
    }

    public abstract class AbilityScript
    {
        public GameWorld World { get; set; }

        public virtual void FrameBegin()
        {

        }

        public virtual void Cast(Entity source, Ability ability, InteractMetaData meta)
        {

        }

        public virtual AbilityStatus ClientCast(Entity source, Ability ability)
        {
            return AbilityStatus.None;
        }

        public virtual AbilityStatus ClientCast(Entity source, Ability ability, Vector3 position)
        {
            return AbilityStatus.None;
        }
    }
}
