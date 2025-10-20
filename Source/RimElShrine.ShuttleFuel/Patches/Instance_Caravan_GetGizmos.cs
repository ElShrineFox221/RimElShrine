using HarmonyLib;
using RimElShrine.Buildings.Shuttle;
using RimWorld.Planet;
using Verse;

namespace RimElShrine.Patches
{
    [HarmonyPatch(typeof(Caravan), nameof(Caravan.GetGizmos))]
    public class Instance_Caravan_GetGizmos
    {
        [HarmonyPostfix]
        public static void Postfix(Caravan __instance, ref IEnumerable<Gizmo> __result)
        {
            var doAutoRefuelGizmosPre = ModsConfig.OdysseyActive && __instance.Shuttle is not null && Find.WorldSelector.NumSelectedObjects == 1;
            if (doAutoRefuelGizmosPre)
            {
                var newList = new List<Gizmo>();
                var added = false;
                foreach (var gizmo in __result.ToList())
                {
                    newList.Add(gizmo);
                    if (!added && gizmo is Command_Action ca && ca.defaultLabel == "CommandRefuelShuttleFromCargo".Translate()) add();
                }
                add();
                void add()
                {
                    if (!added && __instance.Shuttle is not null) 
                    {
                        added = true;
                        AutoRefuelShuttleUtility.TryAddCompAutoRefuel(__instance.Shuttle, __instance, out var autoRefuelComp);
                        newList.AddRange(AutoRefuelShuttleUtility.GetAutoRefuelGizmos(__instance.Shuttle, autoRefuelComp));
                    }
                }
                __result = newList;
            }
        }
    }
}
