using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimElShrine.Traders.EmergencyChemfuelTrader
{
    public class EmergencyChemfuelTrader : ITrader
    {
        protected ShuttleFuelSetting Setting => Settings.GetSetting<ShuttleFuelSetting>();

        public IEnumerable<Thing> Goods => goods;
        public readonly Settlement settlementFrom;
        protected readonly List<Thing> goods = [];

        public TraderKindDef TraderKind { get; protected set; }
        public Faction Faction => settlementFrom.Faction;
        public Caravan? Caravan { get; protected set; }
        public Building_PassengerShuttle Shuttle { get; protected set; }
        public int Goodwill { get; protected set; }
        public float Distance { get; protected set; }
        public string TraderName => settlementFrom?.TraderName ?? Faction.NameColored;
        public bool CanTradeNow => true;
        public TradeCurrency TradeCurrency => TradeCurrency.Silver;

        private static readonly System.Random random = new();
        public EmergencyChemfuelTrader(Building_PassengerShuttle shuttle, Settlement settlement, float distance, int goodwill)
        {
            var caravan = shuttle.GetCaravan();
            TraderKind = DefDatabase<TraderKindDef>.GetNamed("ELS_Trader_EmgChemfuelTrader");
            settlementFrom = settlement;
            Caravan = caravan;
            Shuttle = shuttle;
            Goodwill = goodwill;
            Distance = distance;
            //Get chemfuels;
            var thing = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Chemfuel"));
            var baseCount = shuttle.MaxFuelLevel - shuttle.FuelLevel;
            var count = random.Next(50, Mathf.RoundToInt(baseCount));
            count = Mathf.RoundToInt(count * (random.Next(100, 300) / 100f));
            thing.stackCount = count;
            goods.AddRange([thing]);
            foreach (var sg in TraderKind.stockGenerators)
            {
                goods.AddRange(sg.GenerateThings(caravan?.Tile ?? shuttle.Map.Tile, Faction));
            }
        }

        public int RandomPriceFactorSeed => 1;
        public float TradePriceImprovementOffsetForPlayer => GetPriceOffset();
        private float GetPriceOffset()
        {
            const float max = 0f, min = -1, middle = -0.2f;
            //
            var goodwill = Goodwill + Setting.EmgTraderPriceGoodwillOffset;
            float offset;
            if (goodwill > 40) offset = max;
            else if (goodwill > -20) offset = Mathf.Abs(goodwill - 40) / 60f * middle;
            else offset = Mathf.Abs(goodwill + 20) / 80f * min;
            //
            offset -= Distance / 500f * Setting.EmgTradePriceDistanceFactor;
            offset = Mathf.Clamp(offset, -0.9f, 0);
            return offset;
        }
        public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
        {
            if (Caravan is not null) return Caravan.AllThings.Where(Filter);
            else
            {
                var transport = Shuttle.TryGetComp<CompTransporter>();
                if(transport is not null) return transport.innerContainer.Where(Filter);
                return [];
            }
        }
        private static bool Filter(Thing thing)
            => !(thing is Pawn p && p.IsColonist);


        public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Caravan caravan = playerNegotiator.GetCaravan();
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
            if (thing is Pawn pawn)
            {
                caravan.AddPawn(pawn, true);
                return;
            }
            Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, caravan.PawnsListForReading, null, null);
            if (pawn2 == null)
            {
                Log.Error("Could not find any pawn to give sold thing to.");
                thing.Destroy(DestroyMode.Vanish);
                return;
            }
            if (!pawn2.inventory.innerContainer.TryAdd(thing, true))
            {
                Log.Error("Could not add sold thing to inventory.");
                thing.Destroy(DestroyMode.Vanish);
            }
        }
        public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Caravan caravan = playerNegotiator.GetCaravan();
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
            if (toGive is Pawn pawn)
            {
                CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading, null);
                if (pawn.RaceProps.Humanlike) return;
                goods.Add(pawn);
            }
            goods.Add(thing);
        }
    }
}
