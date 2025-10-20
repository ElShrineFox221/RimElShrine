using Verse;

namespace RimElShrine
{
    public class RimElShrineShuttleMod : RimElShrineMod
    {
        public override string ModName => nameof(RimElShrineShuttleMod);

        public RimElShrineShuttleMod(ModContentPack content) : base(content)
        {

        }
        static RimElShrineShuttleMod()
        {
            Harmony.PatchAll(typeof(RimElShrineShuttleMod).Assembly);
        }
    }
}
