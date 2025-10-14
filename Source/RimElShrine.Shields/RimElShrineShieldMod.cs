using UnityEngine;
using Verse;

namespace RimElShrine
{
    public class RimElShrineShieldMod(ModContentPack content) : RimElShrineMod(content)
    {
        public override string ModName => nameof(RimElShrineShieldMod);
    }

    public class TestWindow : Window
    {
        public TestWindow()
        {
            doWindowBackground = true;
            absorbInputAroundWindow = true;
            forcePause = false;
            layer = WindowLayer.SubSuper;
        }
        private Vector2 pos = Vector2.zero;
        public override void DoWindowContents(Rect inRect)
        {
            ELSLog.Debug($"Do rect={inRect}");
            var newRect = inRect;
            Listing_Standard listing = new();
            newRect.height = 4000;
            Widgets.BeginScrollView(inRect, ref pos, newRect);
            listing.Begin(newRect);
            //
            //Action
            //
            listing.End();
            ELSLog.Debug($"height={listing.CurHeight}");
            Widgets.EndScrollView();
        }
    }
}
