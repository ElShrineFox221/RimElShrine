using RimElShrine;
using RimElShrine.Hediffs.LimbsRegen;
using RimWorld;
using Verse;

namespace RimElShrine.Items.LimbsRegenInjector
{
    public class Comp_LimbsRegenInjector : CompUseEffect
    {
        protected CompProperties_LimbsRegenInjector Props => (CompProperties_LimbsRegenInjector)props;
        protected HediffDef ELS_Hediff_LimbsRegen => DefDatabase<HediffDef>.GetNamed(nameof(ELS_Hediff_LimbsRegen));
        public override void DoEffect(Pawn user)
        {
            var pawn = user;
            if (pawn != null)
            {
                if (Props.isCleaner)
                {
                    var hediff = pawn.health.GetOrAddHediff(ELS_Hediff_LimbsRegen);
                    if (hediff != null) hediff.Severity = 0;
                    if (PawnUtility.ShouldSendNotificationAbout(pawn))
                    {
                        Messages.Message("LimbsRegenInjector_ClearRegenEffectMsg".ES_Translate(pawn), pawn, MessageTypeDefOf.PositiveEvent);
                    }
                }
                else
                {
                    if (pawn.health.AddHediff(ELS_Hediff_LimbsRegen) is HediffWithComps_LimbsRegen hediff) hediff.lastRegenCount = Props.lastRegenCount;
                    if (PawnUtility.ShouldSendNotificationAbout(pawn))
                    {
                        Messages.Message("LimbsRegenInjector_ApplyRegenEffectMsg".ES_Translate(pawn), pawn, MessageTypeDefOf.PositiveEvent);
                    }
                }
            }
        }
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            var disabledReason = Props.isCleaner ? "LimbsRegenInjector_DisabledReason_EffectNotExisted" : "LimbsRegenInjector_DisabledReason_EffectExisted";
            var result = true;
            if (p != null)
            {
                var existed = p.TryGetHediff(ELS_Hediff_LimbsRegen, out _);
                result = !(existed ^ Props.isCleaner);
            }
            if (!result) return disabledReason.ES_Translate();
            return result;
        }
    }
}
