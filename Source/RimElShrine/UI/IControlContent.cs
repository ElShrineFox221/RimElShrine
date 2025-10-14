using UnityEngine;

namespace RimElShrine.UI
{
    public interface IControlContent
    {
        public void DrawControl(ContentControl drawer, Rect rectAll, out Rect rectRemained, out bool skipped);
    }
}
