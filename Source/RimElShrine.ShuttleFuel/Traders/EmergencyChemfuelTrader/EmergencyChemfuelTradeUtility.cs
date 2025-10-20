using RimElShrine.Buildings.Shuttle;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace RimElShrine.Traders.EmergencyChemfuelTrader
{
    public static class EmergencyChemfuelTradeUtility
    {
        private static ShuttleFuelSetting Setting => Settings.GetSetting<ShuttleFuelSetting>();
        public static Caravan? GetCaravan(this Building_PassengerShuttle shuttle)
        {
            var comp = shuttle.TryGetComp<Comp_AutoRefuelShuttle>();
            return comp?.caravan;
        }
        public static PlanetTile? GetPlanetTile(this Building_PassengerShuttle shuttle)
        {
            var comp = shuttle.TryGetComp<Comp_AutoRefuelShuttle>();
            var tile = shuttle.Spawned ? shuttle.Map.Tile : comp?.caravan?.Tile;
            return tile;
        }
        private readonly static Dictionary<PlanetTile, List<EmergencyChemfuelTrader>> emgTradersByTile = [];

        public static IReadOnlyList<EmergencyChemfuelTrader> GetTradersByPlanetTile(PlanetTile tile)
            => emgTradersByTile.TryGetValue(tile, out var list) ? list : [];
        public static void OpenFuelTradeComm(Building_PassengerShuttle shuttle)
        {
            emgTradersByTile.RemoveAll(kp => !Find.WorldObjects.AnyWorldObjectAt(kp.Key));
            //
            var tile = shuttle.GetPlanetTile();
            if(tile.HasValue)
            {
                var got = emgTradersByTile.TryGetValue(tile.Value, out var list);
                if (!got) emgTradersByTile.Add(tile.Value, list = []);
                var tansporter = shuttle.TryGetComp<CompTransporter>();
                var pawns = tansporter?.innerContainer.OfType<Pawn>().ToList() ?? [];
                var caravan = shuttle.GetCaravan();
                if (caravan is not null) pawns.AddRange(caravan.pawns);
                var pawn = SelectNegotiator(pawns);
                if (pawn is not null)
                {
                    var dialog = new Dialog_SelectEmgChemfuelTrader(shuttle, pawn);
                    Find.WindowStack.Add(dialog);
                }
                else Messages.Message("NoNegotiatorNotice".ES_Translate(), MessageTypeDefOf.RejectInput);
            }
        }
        private static Pawn? SelectNegotiator(List<Pawn> candidates)
        {
            if (candidates.Count == 0) return null;
            else
            {
                return candidates.RandomElement();
            }
        }
        public static EmergencyChemfuelTrader? GetNewEmgChemfuelTrader(Building_PassengerShuttle shuttle, Pawn negotiator)
        {
            var caravan = GetCaravan(shuttle);
            var shuttleTile = GetPlanetTile(shuttle);
            List<EmergencyChemfuelTrader> traders;
            if (shuttleTile.HasValue)
            {
                var got = emgTradersByTile.TryGetValue(shuttleTile.Value, out traders);
                if (!got) emgTradersByTile.SetOrAdd(shuttleTile.Value, traders);
            }
            else return null;
            //
            var settlements = Find.WorldObjects.AllWorldObjects
                .Select(wo => wo as Settlement)
                .Where(stm =>
                {
                    var validStm = stm is not null && stm.Faction != Faction.OfPlayer && stm.Tile.LayerDef == shuttle.Tile.LayerDef && !traders.Any(t => t.settlementFrom == stm);
                    return validStm;
                });
            if (!settlements.Any()) return null;
            var stmInfos = settlements
                .Select(stm =>
                {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                    var tile = caravan?.Tile ?? shuttle.Map?.Tile;
                    var distance = tile is null ? float.MaxValue : Find.WorldGrid.ApproxDistanceInTiles(stm.Tile, tile.Value);
                    var faction = stm.Faction;
#pragma warning restore CS8602 // 解引用可能出现空引用。
                    var goodwill = faction.GoodwillWith(negotiator.Faction) / 100f;
                    return (stm, distance, goodwill);
                });
            if (!stmInfos.Any()) return null;
            var settlement = SelectWeighted(stmInfos, Setting.EmgTradeWeightOffset);
            if (settlement.Item1 is null) return null;
            var trader = new EmergencyChemfuelTrader(shuttle, settlement.Item1, settlement.Item2, Mathf.RoundToInt(settlement.Item3 * 100));
            traders.Add(trader);
            return trader;
        }


        private const float SmoothTransitionRange = 10f; 
        private const float BaseDistanceWeight = 50f;
        private const float decayRate = 0.97f;
        private static (Settlement?, float, float) SelectWeighted(IEnumerable<(Settlement thing, float distance, float goodwill)> data,float weightOffset)
        {
            weightOffset = Mathf.Clamp(weightOffset, -1f, 1f);
            var dataList = data.ToList();
            var weights = dataList.Select(item => calcWeight(item.thing, item.distance, item.goodwill, weightOffset)).ToList();
            var totalWeight = weights.Sum(v => v.weight);
            var random = new Random();
            var randomValue = (float)random.NextDouble() * totalWeight;
            var cumulativeWeight = 0f;
            foreach (var w in weights)
            {
                cumulativeWeight += w.weight;
                if (randomValue <= cumulativeWeight) return (w.stm, w.dis, w.gw);
            }
            for (int i = 0; i < dataList.Count; i++)
            {
                cumulativeWeight += weights[i].weight;
                if (randomValue <= cumulativeWeight) return dataList[i];
            }
            return dataList.Last();
            static (Settlement stm, float weight, float dis, float dw, float gw, float gww) calcWeight(Settlement item, float distance, float goodwill, float weightOffset)
            {
                float distanceWeight = calcDistanceWeight(distance);
                float goodwillWeight = (goodwill + 1f) * 50f;
                float distanceFactor = (1f - weightOffset) / 2f;
                float goodwillFactor = (1f + weightOffset) / 2f;
                var weight = distanceWeight * distanceFactor + goodwillWeight * goodwillFactor;
                return (item, Mathf.Pow(weight, 2), distance, distanceWeight, goodwill, goodwillWeight);
                static float calcDistanceWeight(float distance)
                {
                    if (distance <= 1f) return 0;
                    if (distance <= SmoothTransitionRange) return SmoothTransitionRange * BaseDistanceWeight / distance;
                    else return BaseDistanceWeight * Mathf.Pow(decayRate, (distance - SmoothTransitionRange));
                }
            }
        }
    }
}
