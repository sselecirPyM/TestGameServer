using Simulation;
using System.ComponentModel.Composition;
using System.Numerics;

namespace FarmingMod.ItemScripts
{
    [Export(typeof(ItemScript))]
    [ExportMetadata("script", "teleport_item")]
    public class TeleportItem : ItemScript
    {
        public override bool Interact(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            float distance = Vector3.Distance(source.transform.position, position);
            if (distance > specialItem.range)
                return false;
            source.AddBlinkDistance(distance);
            position += Vector3.UnitY;
            //source.transform.position = position;
            World.MoveEntity(source, position, source.transform.rotation);
            World.EmitEvent(new WorldEvent()
            {
                teleport = new TeleportEvent()
                {
                    entityId = source.id,
                    position = position
                }
            });
            return true;
        }

        public override AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem)
        {
            return AbilityStatus.SelectTarget;
        }

        public override AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem, Vector3 position)
        {
            if (Vector3.Distance(source.transform.position, position) > specialItem.range)
                return AbilityStatus.None;
            position += Vector3.UnitY;
            World.MoveEntity(source, position, source.transform.rotation);
            World.EmitEventLocal(new WorldEvent()
            {
                teleport = new TeleportEvent()
                {
                    entityId = source.id,
                    position = position
                }
            });
            return AbilityStatus.Cast;
        }
    }
}
