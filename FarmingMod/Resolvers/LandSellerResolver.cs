using Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace FarmingMod
{
    [Export(typeof(EntityScript))]
    [ExportMetadata("script", "land_seller")]
    public class LandSellerResolver : EntityScript
    {
    }
}
