using Simulation;
using System.ComponentModel.Composition;

namespace FarmingMod.ItemScripts
{
    [Export(typeof(ItemScript))]
    [ExportMetadata("script", "gold_item")]
    public class GoldScript : ItemScript
    {
        public override AbilityStatus ClientInteract(Entity source, int index, SpecialItem specialItem)
        {
            World.EmitEventLocal(new WorldEvent()
            {
                showMenu = new ShowMenuEvent()
                {
                    entityId = source.id,
                    title = "目标达成",
                    type = ShowMenuType.YouWin
                }
            });
            return AbilityStatus.None;
        }
    }
}
