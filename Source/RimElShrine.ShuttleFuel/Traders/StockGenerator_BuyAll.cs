using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimElShrine.Traders
{
    public class StockGenerator_BuyAll : StockGenerator
    {
        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction? faction = null)
        {
            yield break;
        }

        public override bool HandlesThingDef(ThingDef thingDef) => true;
    }
}
