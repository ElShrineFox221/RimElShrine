using RimWorld;
using UnityEngine;
using Verse;

namespace RimElShrine.UI
{
    public static class DoContentHelper
    {
        public const float IntentLength = 10f;
        public static float SilderLabeled(DoContentInfo info, float value, float min, float max, float defaultValue)
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            //get value str
            string toValueString(float val) => string.IsNullOrEmpty(info.ValueFormat) ? val.ToString() : val.ToString(info.ValueFormat);
            var valueStr = toValueString(value);
            var shownValueLabel = info.ValueLabel is null ? valueStr : info.ValueLabel.ES_Translate(valueStr).RawText;
            //labelUntranslated & offset & resetBtn
            var labelRect = info.Rect.LeftPart(info.LabelPct);
            if (info.Intents > 0)
            {
                var intentPixs = info.Intents * IntentLength;
                labelRect.x += intentPixs;
                labelRect.width -= intentPixs;
            }
            Widgets.ButtonText(labelRect, info.Label.ES_Translate(valueStr), false, false);
            //num labelUntranslated
            if (info.ShowValueLabel)
            {
                var numLabelRect = info.Rect.LeftPart(info.LabelPct + info.ValueLabelPct).RightPart(info.ValueLabelPct / (info.LabelPct + info.ValueLabelPct));
                numLabelRect.x += info.HorizontalPadding;
                numLabelRect.width -= info.HorizontalPadding * 2;
                Widgets.Label(numLabelRect, shownValueLabel);
            }
            //invisible button
            var leftRect = info.Rect.LeftPart(info.LabelPct + info.ValueLabelPct);
            leftRect.width += info.HorizontalPadding * 2;
            if (Widgets.ButtonInvisible(leftRect, true) && !Mathf.Approximately(value, defaultValue)) value = defaultValue;
            //tooltip
            if(info.Tooltip is not null)
            {
                var oTip = string.IsNullOrEmpty(info.Tooltip) ? string.Empty : $"{info.Tooltip.ES_Translate(valueStr)}\n\n";
                var additional = $"{"max".ES_Translate()}: {toValueString(max)}\n{"min".ES_Translate()}: {toValueString(min)}\n{"default".ES_Translate()}: {toValueString(defaultValue)}";
                oTip += additional;
                TooltipHandler.TipRegion(leftRect, oTip);
            }
            //slider
            Text.Anchor = TextAnchor.UpperLeft;
            var r = Widgets.HorizontalSlider(info.Rect.RightPart(1 - info.LabelPct - info.ValueLabelPct), value, min, max, true);
            Text.Anchor = anchor;
            return r;
        }
        public static bool CheckboxLabeled(DoContentInfo info, bool value, bool defaultValue)
        {
            const float checkBoxWidth = 24f;
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            //get value str
            var valueStr = value.ToString();
            var shownValueLabel = info.ValueLabel is null ? valueStr : info.ValueLabel.ES_Translate(valueStr).RawText;
            //labelUntranslated & offset & resetBtn
            var labelRect = info.Rect.LeftPart(info.LabelPct);
            if (info.Intents > 0)
            {
                var intentPixs = info.Intents * IntentLength;
                labelRect.x += intentPixs;
                labelRect.width -= intentPixs;
            }
            Widgets.ButtonText(labelRect, info.Label.ES_Translate(valueStr), false, false);
            //num labelUntranslated
            if (info.ShowValueLabel)
            {
                var numLabelRect = info.Rect.LeftPart(info.LabelPct + info.ValueLabelPct).RightPart(info.ValueLabelPct / (info.LabelPct + info.ValueLabelPct));
                numLabelRect.x += info.HorizontalPadding;
                numLabelRect.width -= info.HorizontalPadding * 2;
                Widgets.Label(numLabelRect, shownValueLabel);
            }
            //invisible button
            var leftRect = info.Rect.LeftPart(info.LabelPct + info.ValueLabelPct);
            leftRect.width += info.HorizontalPadding * 2;
            //tooltip
            if (info.Tooltip is not null)
            {
                var oTip = string.IsNullOrEmpty(info.Tooltip) ? string.Empty : $"{info.Tooltip.ES_Translate(valueStr)}\n\n";
                var additional = $"{"default".ES_Translate()}: {defaultValue}";
                oTip += additional;
                TooltipHandler.TipRegion(leftRect, oTip);
            }
            //slider
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.ToggleInvisibleDraggable(info.Rect, ref value, true, false);
            Widgets.CheckboxDraw(info.Rect.x + info.Rect.width - checkBoxWidth, info.Rect.y + (info.Rect.height - checkBoxWidth) / 2f, value, false, checkBoxWidth, null, null);
            Text.Anchor = anchor;
            return value;
        }
        public static void TitleLabel(DoContentInfo info)
        {
            var cachedFontSize = Text.CurFontStyle.fontSize;
            Text.CurFontStyle.fontSize = info.FontSize;
            var intentPixs = info.Intents * IntentLength;
            var rect = info.Rect;
            rect.x += intentPixs;
            rect.width -= intentPixs;
            var str = info.Label.ES_Translate();
            var h = Text.CalcHeight(str, rect.width);
            rect.height = h;
            Widgets.Label(rect, str.Colorize(info.Color));
            if (info.Tooltip is not null && info.Tooltip != string.Empty)
            {
                TooltipHandler.TipRegion(rect, info.Tooltip.ES_Translate());
            } 
            Text.CurFontStyle.fontSize = cachedFontSize;
            info.Rect = rect;
        }
    }
}
