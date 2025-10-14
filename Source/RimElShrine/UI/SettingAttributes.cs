using System;
using UnityEngine;

namespace RimElShrine.UI
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingAttribute : Attribute
    {
        public bool IgnoreSetting = false;
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public abstract class SettingItemAttribute(string? catalog = null, string? parentName = null, string? format = null) : Attribute
    {
        public string Catalog = catalog ?? string.Empty;
        public Type? ParentDeclaringType = null;
        public string ParentName = parentName ?? string.Empty;
        public string? ValueLabel = format;
        public abstract void PreDoControl(ContentControl control, SettingItem si, Rect rect, out float height);
        public abstract void DoControl(ContentControl control, SettingItem si, Rect rect, float height);
        public virtual void DoMerge(SettingItemAttribute other)
        {
            if (!string.IsNullOrEmpty(other.Catalog)) Catalog = other.Catalog;
            if (other.ParentDeclaringType is not null) ParentDeclaringType = other.ParentDeclaringType;
            if (!string.IsNullOrEmpty(other.ParentName)) ParentName = other.ParentName;
            if (!string.IsNullOrEmpty(other.ValueLabel)) ValueLabel = other.ValueLabel;
        }
        public override string ToString()
            => $"[cata={Catalog}, parentT={ParentDeclaringType}, parentN={ParentName}, format={ValueLabel}]";
        protected static float DoClampRound(float v, float min, float max, float step)
            => Mathf.Round(Mathf.Clamp((float)v, min, max) / step) * step;
    }
    public class SettingItemIntAttribute(int min, int max, int step, string? catalog = null, string? parentName = null, string? format = null, string? numFormat = null) : SettingItemAttribute(catalog, parentName, format)
    {
        public int Min = min;
        public int Max = max;
        public int Step = step;
        public string ValueFormat = numFormat ?? string.Empty;

        public override void DoControl(ContentControl control, SettingItem si, Rect rect, float height)
        {
            var value = (int)(si.GetValue() ?? 0);
            var defaultValue = (int)(si.Data.Default ?? 0);
            var isia = si.SettingItemAttr as SettingItemIntAttribute;
            if (isia is not null)
            {
                var info = new DoContentInfo(rect)
                {
                    Label = si.Data.Label,
                    ValueLabel = isia.ValueLabel,
                    ValueFormat = isia.ValueFormat,
                    Tooltip = si.TooltipKey,
                    Intents = control.VisualTreeDepth,
                };
                var rv = DoContentHelper.SilderLabeled(info, value, isia.Min, isia.Max, defaultValue);
                rv = DoClampRound(rv, isia.Min, isia.Max, isia.Step);
                si.SetValue((int)rv);
            }
        }
        public override void PreDoControl(ContentControl control, SettingItem si, Rect rect, out float height)
        {
            height = 30f;
        }
        public override void DoMerge(SettingItemAttribute other)
        {
            base.DoMerge(other);
            if(other is SettingItemIntAttribute siia)
            {
                Min = siia.Min;
                Max = siia.Max;
                Step = siia.Step;
                ValueFormat = siia.ValueFormat;
            }
        }
    }
    public class SettingItemFloatAttribute(float min, float max, float step, string? catalog = null, string? parentName = null, string? format = null, string? numFormat = null) : SettingItemAttribute(catalog, parentName, format)
    {
        public float Min = min;
        public float Max = max;
        public float Step = step;
        public string ValueFormat = numFormat ?? "P0";

        public override void DoControl(ContentControl control, SettingItem si, Rect rect, float height)
        {
            var value = (float)(si.GetValue() ?? 0);
            var defaultValue = (float)(si.Data.Default ?? 0);
            var fsia = si.SettingItemAttr as SettingItemFloatAttribute;
            if (fsia is not null) 
            {
                var info = new DoContentInfo(rect)
                {
                    Label = si.Data.Label,
                    ValueLabel = fsia.ValueLabel,
                    ValueFormat = fsia.ValueFormat,
                    Tooltip = si.TooltipKey,
                    Intents = control.VisualTreeDepth,
                };
                var rv = DoContentHelper.SilderLabeled(info, value, fsia.Min, fsia.Max, defaultValue);
                rv = DoClampRound(rv, fsia.Min, fsia.Max, fsia.Step);
                si.SetValue(rv);
            }
        }

        public override void PreDoControl(ContentControl control, SettingItem si, Rect rect, out float height)
        {
            height = 30f;
        }
        public override void DoMerge(SettingItemAttribute other)
        {
            base.DoMerge(other);
            if (other is SettingItemFloatAttribute sifa)
            {
                Min = sifa.Min;
                Max = sifa.Max;
                Step = sifa.Step;
                ValueFormat = sifa.ValueFormat;
            }
        }
    }
    public class SettingItemBoolAttribute(string? catalog = null, string? parentName = null, string? format = null) : SettingItemAttribute(catalog, parentName, format)
    {
        public override void DoControl(ContentControl control, SettingItem si, Rect rect, float height)
        {
            var value = (bool)(si.GetValue() ?? false); 
            var defaultValue = (bool)(si.Data.Default ?? false);
            var info = new DoContentInfo(rect)
            {
                Label = si.Data.Label,
                ShowValueLabel = false,
                Tooltip = si.TooltipKey,
                Intents = control.VisualTreeDepth,
            };
            var rv = DoContentHelper.CheckboxLabeled(info, value, defaultValue);
            si.SetValue(rv);
            if (!rv)
            {
                foreach (var child in control.Children)
                {
                    child.IsVisible = false;
                }
            }
            else
            {
                foreach (var child in control.Children)
                {
                    child.IsVisible = true;
                }
            }
        }

        public override void PreDoControl(ContentControl control, SettingItem si, Rect rect, out float height)
        {
            height = 30f;
        }
    }
    public class SettingItemStringAttribute(string? catalog = null, string? parentName = null, string? format = null) : SettingItemAttribute(catalog, parentName, format)
    {
        public int FontSize = 20;
        public override void DoControl(ContentControl control, SettingItem si, Rect rect, float height)
        {
            var value = si.GetValue()?.ToString() ?? "null";
            var info = new DoContentInfo(rect) { Label = value, Tooltip = si.TooltipKey, FontSize = FontSize };
            DoContentHelper.TitleLabel(info);
        }
        public override void PreDoControl(ContentControl control, SettingItem si, Rect rect, out float height)
        {
            height = 2 + FontSize * 2;
        }
    }
}
