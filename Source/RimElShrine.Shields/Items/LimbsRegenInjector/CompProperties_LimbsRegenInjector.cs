using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimElShrine.Items.LimbsRegenInjector
{
    public class CompProperties_LimbsRegenInjector : CompProperties_UseEffect
    {
        public int lastRegenCount = -1;
        public bool isFastRegen = false;
        public bool isCleaner = false;
        public CompProperties_LimbsRegenInjector()
        {
            compClass = typeof(Comp_LimbsRegenInjector);
        }
    }
}
