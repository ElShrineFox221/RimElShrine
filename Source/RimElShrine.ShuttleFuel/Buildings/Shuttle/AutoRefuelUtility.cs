using RimElShrine.Reflection;
using RimElShrine.Traders.EmergencyChemfuelTrader;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimElShrine.Buildings.Shuttle
{
    [StaticConstructorOnStartup]
    public static class AutoRefuelShuttleUtility
    {
        private static Texture2D? setTargetFuelLevelCommand;
        public static Texture2D SetTargetFuelLevelCommand => setTargetFuelLevelCommand ??= ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel", true);

        private static Texture2D? emgTradeCommand;
        public static Texture2D EmgTradeCommand => emgTradeCommand ??= ContentFinder<Texture2D>.Get("Commands/ShuttleFuel_CallFuel");

        public static void InstantRefuel(Building_PassengerShuttle shuttle, Caravan? caravan, float to)
        {
            var fuelNeeded = Mathf.RoundToInt(Mathf.Min(to, shuttle.MaxFuelLevel) - shuttle.FuelLevel);
            if (fuelNeeded <= 0) return;
            var fromCaravan = caravan is not null;
            var fuelHad = Mathf.RoundToInt(fromCaravan ? CaravanShuttleUtility.FuelInCaravan(caravan) : (float)(shuttle.GetType().GetCachedMethodInfo("FuelInShuttle")?.Invoke(shuttle, []) ?? 0f));
            var fuelCount = Math.Min(fuelHad, fuelNeeded);
            if (fuelCount > 0)
            {
                if (fromCaravan) CaravanShuttleUtility.ConsumeFuelFromCaravanInventory(caravan, fuelCount);
                else shuttle.GetType().GetCachedMethodInfo("ConsumeFuelFromInventory")?.Invoke(shuttle, [fuelCount]);
                shuttle.RefuelableComp.Refuel((float)fuelCount);
                var faction = caravan is not null ? caravan.Faction : shuttle.Faction;
                if(faction == Faction.OfPlayer)
                {
                    var refueledFull = fuelHad >= fuelNeeded;
                    var lookTarget = fromCaravan ? new LookTargets(caravan) : new LookTargets(shuttle);
                    var msg = refueledFull ? "AutoRefueledFullMsg" : "AutoRefueledMsg";
                    Messages.Message(msg.ES_Translate(fuelCount, to), lookTarget, MessageTypeDefOf.NeutralEvent);
                }
            }
        }
        public static ShuttleFuelSetting Setting => Settings.GetSetting<ShuttleFuelSetting>();
        public static IEnumerable<Gizmo> GetAutoRefuelGizmos(Building_PassengerShuttle shuttle, Comp_AutoRefuelShuttle refuelComp)
        {
            var command_Toggle = new Command_Toggle
            {
                icon = Building_PassengerShuttle.RefuelFromCargoIcon.Texture,
                defaultLabel = "AutoRefuel".ES_Translate(),
                defaultDesc = "AutoRefuelDesc".ES_Translate(),
                isActive = () => refuelComp.autoRefuel,
                toggleAction = delegate
                {
                    refuelComp.autoRefuel = !refuelComp.autoRefuel;
                },
            };
            yield return command_Toggle;
            var command_Action = new Command_Action()
            {
                defaultLabel = "AutoRefuelLevel".ES_Translate(),
                defaultDesc = "AutoRefuelLevelDesc".ES_Translate(),
                icon = SetTargetFuelLevelCommand,
                action = () =>
                {
                    Dialog_Slider window = new(val => "RefuelShuttleCount".Translate(val), 0, Mathf.RoundToInt(shuttle.MaxFuelLevel), delegate (int count)
                    {
                        refuelComp.autoRefuelAmount = count;
                    }, Mathf.RoundToInt(refuelComp.autoRefuelAmount));
                    Find.WindowStack.Add(window);
                }
            };
            yield return command_Action;
            var command_Action_EmgTrade = new Command_Action()
            {
                defaultLabel = "EmgTrade".ES_Translate(),
                defaultDesc = "EmgTradeDesc".ES_Translate(),
                icon = EmgTradeCommand,
                action = () =>
                {
                    EmergencyChemfuelTradeUtility.OpenFuelTradeComm(shuttle);
                },
            };
            if (!NoFuel(shuttle)) command_Action_EmgTrade.Disable("EmgTradeDisabledReason".ES_Translate());
            yield return command_Action_EmgTrade; 
            yield break;
        }
        private static ThingDef? chemfuelDef;
        private static ThingDef ChemfuelDef => chemfuelDef ??= DefDatabase<ThingDef>.GetNamed("Chemfuel");
        private static bool NoFuel(Building_PassengerShuttle shuttle)
        {
            var fuels = 0f;
            if (shuttle.Spawned)
            {
                var transport = shuttle.TryGetComp<CompTransporter>();
                if (transport is not null) fuels += transport.innerContainer.Sum(x => x.def == ChemfuelDef ? x.stackCount : 0);
            }
            else
            {
                var comp = shuttle.TryGetComp<Comp_AutoRefuelShuttle>();
                if(comp is not null)
                {
                    var caravan = comp.caravan;
                    fuels += CaravanShuttleUtility.FuelInCaravan(caravan);
                }
            }
            fuels += shuttle.FuelLevel;
            var noSuffient = fuels < Setting.EmgTradeThresholdInt || fuels < shuttle.MaxFuelLevel * Setting.EmgTradeThreshold;
            return noSuffient;
        }

        public static bool TryAddCompAutoRefuel(Building_PassengerShuttle shuttle, Caravan? caravan, out Comp_AutoRefuelShuttle autoRefuelComp)
        {
            var add = false;
            autoRefuelComp = shuttle.TryGetComp<Comp_AutoRefuelShuttle>();
            if (autoRefuelComp is null)
            {
                add = true;
                autoRefuelComp = new();
                shuttle.AddComps(autoRefuelComp);
            }
            autoRefuelComp.caravan = caravan;
            return add;
        }
    }
}
