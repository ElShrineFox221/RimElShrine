using RimElShrine.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RimElShrine.UI
{
    public class SettingItem : IControlContent
    {
        #region SettingItem infos
        protected readonly static Dictionary<ExposeData, SettingItemAttribute> mergedSIADatabase = [];
        private static string cachedSIACata = Constants.CommonCata;
        public string TooltipKey;
        private SettingItemAttribute? settingItemAttr = null;
        public SettingItemAttribute SettingItemAttr
        {
            get => settingItemAttr ??= GetMergedSIA();
        }
        public SettingItem(ExposeData data)
        {
            TooltipKey = $"{data.Label}_Tooltip";
            Data = data;
            settingItemAttr = GetMergedSIA();
        }
        public ExposeData Data { get; }

        protected SettingItemAttribute GetMergedSIA()
        {
            var got = mergedSIADatabase.TryGetValue(Data, out var result);
            if (!got)
            {
                var sias = Data.Info.GetCustomAttributes<SettingItemAttribute>(true);
                foreach (var other in sias)
                {
                    if (string.IsNullOrEmpty(other.Catalog)) other.Catalog = cachedSIACata;
                    else cachedSIACata = other.Catalog;
                    //
                    if (result is null) result = other;
                    else
                    {
                        Type curType = result.GetType(), otherType = other.GetType();
                        bool b0 = curType.IsAssignableFrom(otherType);
                        if (curType.IsAssignableFrom(otherType))
                        {
                            result.DoMerge(other);
                            if (curType != otherType)
                            {
                                other.DoMerge(result);
                                result = other;
                            }
                        }
                        else if (otherType.IsAssignableFrom(curType)) result.DoMerge(other);
                        else throw new Exception($"Cannot merge {otherType} to {curType}, exposeData={Data}");
                    }
                }
                result.ParentDeclaringType ??= Data.Info.DeclaringType;
            }
            if (result is null) throw new();
            if (!got) mergedSIADatabase.Add(Data, result);
            return result;
        }
        #endregion

        #region UI
        public static float verticalSpace = 5f;
        public void DrawControl(ContentControl drawer, Rect rectAll, out Rect rectRemained, out bool skipped)
        {
            skipped = SettingsWindow.ControlsPreSkips.TryGetValue(drawer, out var skip) && skip;
            if (!skipped)
            {
                var sia = SettingItemAttr;
                sia.PreDoControl(drawer, this, rectAll, out var h);
                var rectDraw = new Rect(rectAll.x, rectAll.y, rectAll.width, h);
                sia.DoControl(drawer, this, rectDraw, h);
                rectDraw.height += verticalSpace;
                rectAll = new Rect(rectAll.x, rectAll.y + rectDraw.height, rectAll.width, rectAll.height - rectDraw.height);
            }
            rectRemained = rectAll;
        }
        public override string ToString()
           => $"[data={Data}, mergedSIA={settingItemAttr}]";
        #endregion
    }
}
