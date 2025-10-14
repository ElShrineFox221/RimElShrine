using UnityEngine;

namespace RimElShrine.UI
{
    public class DoContentInfo(Rect rect)
    {
        public Rect Rect = rect;
        public Color Color = Color.white;
        public int FontSize = 14;
        public string Label = string.Empty;
        public float LabelPct = 0.4f;
        /// <summary>
        /// null: No tooltip
        /// empty: Auto add value tooltips, including tips such as max, min, default
        /// </summary>
        public string? Tooltip = null;
        public bool ShowValueLabel = true;
        public Color ValueColor = Color.white;
        public float ValueLabelPct = 0.1f;
        /// <summary>
        /// ValueLabel to invoke value.ToString(format) to create valueStr;
        /// empty: use value.ToString();
        /// </summary>
        public string ValueFormat = string.Empty;
        /// <summary>
        /// Label to invoke valueLabel.Translate(valueStr);
        /// null: use valueStr
        /// </summary>
        public string? ValueLabel = null;
        public float HorizontalPadding = 5f;
        public int Intents = 0;
    }
}
