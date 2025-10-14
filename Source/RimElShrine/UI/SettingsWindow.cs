using RimElShrine.Data;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RimElShrine.UI
{
    public sealed class SettingsWindow : Window
    {
        private static int selectedPannelIndex = 0;
        public static int SelectedPannelIndex => selectedPannelIndex;
        private static IReadOnlyList<Control> uiForest = [];
        private static IReadOnlyList<string> catalogs = [];
        private static readonly Dictionary<Control, bool> controlsPreSkips = [];
        public static IReadOnlyDictionary<Control, bool> ControlsPreSkips => controlsPreSkips;
        
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight = int.MaxValue;
        private string cachedCata = string.Empty;

        public SettingsWindow()
        {
            doWindowBackground = true;
            absorbInputAroundWindow = true;
            forcePause = false;
            layer = WindowLayer.SubSuper;
            //rebuild ui forest set;
            controlsPreSkips.Clear();
            uiForest = Settings.UIForest;
            catalogs = [.. Settings.SettingItemsByLabel.Values.GroupBy(item => item.SettingItemAttr.Catalog).Select(group => group.Key)];
            selectedPannelIndex /= catalogs.Count;
            UpdatePreSkipsTab();
        }
        private void UpdatePreSkipsTab()
        {
            uiForest.TraverseDLR(node =>
            {
                controlsPreSkips.SetOrAdd(node, preSkipsCond(node, catalogs[selectedPannelIndex]));
                return TreeHelper.TraversalState.Continue;
            });
            static bool preSkipsCond(Control uiTreeNode, string catalog)
            {
                var result = false;
                if (uiTreeNode is ContentControl contentControl)
                {
                    var si = (SettingItem)contentControl.Content;
                    var siIsTitle = (si.Data.Info is FieldInfo fi && fi.IsInitOnly || si.Data.Info is PropertyInfo pi && !pi.CanWrite)
                        && typeof(string).IsAssignableFrom(si.Data.DataType);
                    if (siIsTitle)
                    {
                        if (catalog == Constants.CommonCata) result = true;
                        else
                        {
                            var any = uiTreeNode.Children.All(c => preSkipsCond(c, catalog));
                            result = any;
                        }
                    }
                    else result = si.SettingItemAttr.Catalog != catalog;
                }
                return result;
            }
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            const float titleHeight = 30f;
            //Get elements rect & tabs
            var tabs = new List<TabRecord>();
            var tabsRect = new Rect(inRect.x, inRect.y + titleHeight, inRect.width, inRect.height + 10f);
            var currentCata = string.Empty;
            for (int i = 0; i < catalogs.Count; i++)
            {
                var cata = catalogs[i];
                var isSelected = SelectedPannelIndex == i;
                if (isSelected) currentCata = cata;
                int index = i;
                tabs.Add(new TabRecord(cata.ES_Translate(), () =>
                {
                    selectedPannelIndex = index;
                    currentCata = catalogs[index];
                    UpdatePreSkipsTab();
                    //reset scroll
                    scrollViewHeight = int.MaxValue;
                    scrollPosition = Vector2.zero;
                }, isSelected));
            }
            //Draw
            Widgets.DrawMenuSection(tabsRect);
            TabDrawer.DrawTabs(tabsRect, tabs);
            DoPanelContent(tabsRect, currentCata);
            //
            Text.Anchor = TextAnchor.UpperLeft;
        }
        public void DoPanelContent(Rect inRect, string cata)
        {
            //Reset scroll position & height when pannel changes
            if(cachedCata != cata)
            {
                scrollViewHeight = int.MaxValue;
                cachedCata = cata;
                scrollPosition.y = 0;
            }
            //Do top btn
            const float margin = 5f, scrollBarWidth = 20f, topBtnRectHeight = 30f;
            inRect = inRect.ContractedBy(margin);
            var topBtnRect = inRect.TopPart(topBtnRectHeight / inRect.height);
            if (Widgets.ButtonText(topBtnRect, "ResetAll".ES_Translate()))
            {
                var grouped = Settings.SettingItemsByLabel.Values.GroupBy(item => item.SettingItemAttr.Catalog);
                var items = grouped.Where(g => g.Key == cata).FirstOrDefault();
                if(items is not null)
                {
                    foreach (var item in items) item.SetValue(item.Data.Default);
                }
            }
            inRect = inRect.BottomPart(1 - topBtnRectHeight / inRect.height);
            //Do items
            var outRect = new Rect(inRect.x + scrollBarWidth, inRect.y, inRect.width - scrollBarWidth, inRect.height);
            var viewRect = new Rect(outRect.x, outRect.y, outRect.width - scrollBarWidth, scrollViewHeight);
            var newRect = Rect.zero;
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            //
            foreach (var uiTree in uiForest)
            {
                uiTree.DrawVisualTree(viewRect, out viewRect, 0);
            }
            var drawnHeight = viewRect.y - outRect.y + 5f;
            //
            Widgets.EndScrollView();

            scrollViewHeight = drawnHeight;
        }
    }
}
