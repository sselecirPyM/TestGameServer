using Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace FarmingMod
{
    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "furniture")]
    public class FurnitureResolver : EntityScript
    {
        public override void Interact(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {

        }
        public override bool ClientInteract(Entity source, Entity entity, InteractType interactType, InteractMetaData meta)
        {
            return false;
        }
    }
}
