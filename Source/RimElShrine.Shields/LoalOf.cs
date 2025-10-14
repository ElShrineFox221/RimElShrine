using RimWorld;
using Verse;

namespace RimElShrine
{
    [DefOf]
    public static class LocalDefOf
    {
#pragma warning disable CS8618
        static LocalDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(LocalDefOf));
#pragma warning restore CS8618 

        #region SoundDefs
        public static SoundDef MortarShield_Ambience;
        #endregion

        #region EffectDefs
        public static EffecterDef MortarShieldGenerator_Reactivate;
        #endregion
    }
}
