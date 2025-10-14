using UnityEngine;

namespace RimElShrine.UI
{
    public class ContentControl(IControlContent content) : Control
    {
        public IControlContent Content { get; set; } = content;
        protected override void DrawControl(Rect rectAll, out Rect rectRemained, out bool skipped)
        {
            Content.DrawControl(this, rectAll, out rectRemained, out skipped);
        }
        public override string ToString()
            => $"[items={Children.Count}, parent={Parent}, content{Content}]";
    }
}
