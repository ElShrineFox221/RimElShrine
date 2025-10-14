using System;
using UnityEngine;
using Verse;

namespace RimElShrine.UI
{
    public class Label(string text, bool translated = false) : ContentControl(new LabelContent())
    {
        public bool Translated { get; set; } = translated;
        protected string text = text;
        protected string tooltip = string.Empty;
        public Func<string>? TextSource;
        public Func<string>? TooltipSource;
        public string Text
        {
            get => string.IsNullOrEmpty(text) ? (TextSource?.Invoke() ?? string.Empty) : text;
            set => text = value;
        }
        public string Tooltip
        {
            get => string.IsNullOrEmpty(tooltip) ? (TooltipSource?.Invoke() ?? string.Empty) : tooltip;
            set => tooltip = value;
        }
        public Color Color { get; set; } = Color.white;
        protected LabelContent TextContent => (LabelContent)Content;

        public Label(Func<string>? textSource = null, bool translated = false) : this(string.Empty, translated) 
            => this.TextSource = textSource;

        protected class LabelContent() : IControlContent
        {
            public void DrawControl(ContentControl drawer, Rect rectAll, out Rect rectRemained, out bool skipped)
            {
                skipped = false;
                rectRemained = rectAll;
                if(drawer is Label label)
                {
                    var rect = rectAll.TopPart(30 / rectAll.height);
                    var info = new DoContentInfo(rect) { Rect = rect, Label = label.Text, Tooltip = label.Tooltip, Color = label.Color, Intents = label.VisualTreeDepth};
                    ELSLog.Debug($"Doing label, intents={label.VisualTreeDepth}");
                    DoContentHelper.TitleLabel(info);
                    rectRemained = rectAll.BottomPart(1 - info.Rect.height / rectAll.height);
                }
            }
        }
    }
}
