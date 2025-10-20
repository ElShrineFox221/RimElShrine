using HarmonyLib;
using RimElShrine.Buildings.Shuttle;
using RimWorld;
using Verse;

namespace RimElShrine.Patches
{
    [HarmonyPatch(typeof(Building_PassengerShuttle), nameof(Building_PassengerShuttle.GetGizmos))]
    public class Instance_PassengerShuttle_GetGizmos
    {
        [HarmonyPostfix]
        public static void Postfix(Building_PassengerShuttle __instance, ref IEnumerable<Gizmo> __result)
            => AutoRefuelShuttleUtility.TryAddCompAutoRefuel(__instance, null, out _);
    }
}
