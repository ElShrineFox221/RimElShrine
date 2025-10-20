using RimElShrine.Data;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimElShrine.Buildings.Shuttle
{
    [StaticConstructorOnStartup]
    public class Comp_AutoRefuelShuttle : ThingComp
    {
        public Comp_AutoRefuelShuttle() : base()
        {

        }
        [ExposeData] public bool autoRefuel = false;
        [ExposeData] public float autoRefuelAmount = 0;
        [ExposeData] public int emgTradeCooldownTicks = 0;
        public Caravan? caravan = null;

        public Building_PassengerShuttle? Shuttle => parent as Building_PassengerShuttle;
        public override void CompTick()
        {
            base.CompTick();
            if (emgTradeCooldownTicks > 0) emgTradeCooldownTicks--;
            if (Shuttle is not null && autoRefuel)
            {
                if (Shuttle.Spawned)
                {
                    AutoRefuelShuttleUtility.InstantRefuel(Shuttle, null, autoRefuelAmount);
                    caravan = null;
                }
                else AutoRefuelShuttleUtility.InstantRefuel(Shuttle, caravan, autoRefuelAmount);
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var item in base.CompGetGizmosExtra()) yield return item;
            if(Shuttle is not null)
            {
                foreach (var g in AutoRefuelShuttleUtility.GetAutoRefuelGizmos(Shuttle, this)) yield return g;
            }
            yield break;
        }

        public override void PostExposeData() => this.ExposeAttributedData();
    }
}
