using HarmonyLib;
using Verse;

namespace RimElShrine.Patches
{
    [HarmonyPatch(typeof(ModSettings), nameof(ModSettings.Write))]
    public class Instance_ModSettings_Write
    {
        [HarmonyPrefix]
        public static bool Prefix(ModSettings __instance)
        {
            if (__instance is RimElShrineModSetting settings)
            {
                settings.Write();
                return false;
            }
            return true;
        }
    }
}
