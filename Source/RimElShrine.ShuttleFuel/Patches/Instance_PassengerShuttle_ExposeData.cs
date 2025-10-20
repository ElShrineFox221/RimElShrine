using HarmonyLib;
using RimElShrine.Buildings.Shuttle;
using RimWorld;
using Verse;

namespace RimElShrine.Patches
{
    [HarmonyPatch(typeof(Building_PassengerShuttle), nameof(Building_PassengerShuttle.ExposeData))]
    public class Instance_PassengerShuttle_ExposeData
    {
        [HarmonyPostfix]
        public static void Postfix(Building_PassengerShuttle __instance)
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                AutoRefuelShuttleUtility.TryAddCompAutoRefuel(__instance, null, out var comp);
                comp.PostExposeData();
            }
        }
    }
}
