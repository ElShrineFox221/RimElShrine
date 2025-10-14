using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    internal class ShieldNetResAllocateWindow : Window
    {
        private readonly ShieldNet shieldNet;
        public ThingWithComps compParentThing;
        private ShieldUpgrade upgrade = new();
        public ShieldNetResAllocateWindow(ShieldNet shieldNet, ThingWithComps compParentThing)
        {
            this.compParentThing = compParentThing;
            this.shieldNet = shieldNet;
            foreach (ShieldUpgradeResource upgradeResource in ShieldUpgradeResourceHelper.Enums) upgrade[upgradeResource] = shieldNet.GetUpgradeResources(upgradeResource);

            forcePause = true;
            doCloseButton = false;
        }
        private const string nullStr = "<null>";
        public override void DoWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            list.Label("ShieldNet_AllocateWindow_Title".ES_Translate());
            Text.Font = GameFont.Small;
            //
            upgrade ??= new ShieldUpgrade();
            var shieldNet = this.shieldNet;
            /*if (shieldNet is null)
            {
                try
                {
                    var parent = compParentThing;
                    var compPower = parent?.GetComp<CompPower>();
                    var powerNet = compPower?.PowerNet;
                    shieldNet = ShieldNet.shieldNets.GetShieldNetByPowerNet(powerNet);
                    string msg = $"parent={parent?.ToString() ?? nullStr}, compPower={compPower?.ToString() ?? nullStr}, powerNet={powerNet?.ToString() ?? nullStr}, shieldNet={shieldNet?.ToString() ?? nullStr}";
                    if (shieldNet == null) ELSLog.Error(msg);
                    else ELSLog.Warn(msg);
                }
                catch (Exception e)
                {
                    ELSLog.Error(e.Message);
                }
            }*/
            list.Label("ShieldNet_ResourcesSummary".ES_Translate(upgrade.Resources, shieldNet.Resources));
            list.Gap();
            foreach (ShieldUpgradeResource resource in ShieldUpgradeResourceHelper.Enums)
            {
                var rect = list.GetRect(40f);
                var tempInt = upgrade[resource];
                var tempIntCopy = tempInt;
                DrawResources(rect, resource, ref tempInt, shieldNet.Resources);
                var maxResourcesToAllocate = shieldNet.Resources - upgrade.Resources;
                var changes = tempInt - tempIntCopy;
                if (maxResourcesToAllocate > 0 || changes < 0)
                {
                    if (maxResourcesToAllocate < changes) changes = maxResourcesToAllocate;
                    upgrade[resource] = tempIntCopy + changes;
                }
            }
            int step = 0;
            try
            {
                var buttonArea = list.GetRect(30f);
                step++;//1
                const int areas = 3;
                var areaWidth = buttonArea.width / areas;
                var leftArea = new Rect(buttonArea.x, buttonArea.y, areaWidth, buttonArea.height);
                step++;
                //2
                var centerArea = new Rect(buttonArea.x + areaWidth, buttonArea.y, areaWidth, buttonArea.height);
                step++;
                //3
                var rightArea = new Rect(buttonArea.x + areaWidth * 2, buttonArea.y, areaWidth, buttonArea.height);
                step++;
                //4
                if (Widgets.ButtonText(leftArea, "ShieldNet_AllocateWindow_Apply".ES_Translate()))
                {
                    shieldNet.ResetAllocatedResources();
                    foreach (ShieldUpgradeResource upgradeResource in ShieldUpgradeResourceHelper.Enums)
                    {
                        shieldNet.TryAllocateResources(upgradeResource, upgrade[upgradeResource], out _);
                    }
                    Close();
                }
                step++;
                //5
                if (Widgets.ButtonText(centerArea, "ShieldNet_AllocateWindow_Reposition".ES_Translate()))
                {
                    foreach (ShieldUpgradeResource upgradeResource in ShieldUpgradeResourceHelper.Enums) upgrade[upgradeResource] = shieldNet.GetUpgradeResources(upgradeResource);
                }
                step++;
                //6
                if (Widgets.ButtonText(rightArea, "ShieldNet_AllocateWindow_Cancel".ES_Translate()))
                {
                    Close();
                }
                step++;
                //7
            }
            catch (Exception e)
            {
                Log.Error($"step={step}, " + e.Message);
            }
            //
            Text.Anchor = TextAnchor.UpperLeft;
            //windowRect = new Rect(windowRect.x, windowRect.y, windowRect.width, list.CurHeight + 2 * Margin);
        }
        private static readonly Color backColor = new(0f / 255, 113f / 255, 255f / 255, 150f / 255);
        public static void DrawResources(Rect rect, ShieldUpgradeResource upgradeResource, ref int resources, int max, int min = 0, float actualHeight = 24f)
        {
            var leftButtonRect = new Rect(rect.x, rect.y, actualHeight, actualHeight);
            var rightButtonRect = new Rect(rect.x + rect.width - actualHeight, rect.y, actualHeight, actualHeight);
            var backRect = new Rect(rect.x + actualHeight, rect.y, rect.width - 2 * actualHeight, actualHeight);
            var coloredRect = new Rect(backRect.x, backRect.y, backRect.width * (resources / (float)max), backRect.height);
            Text.Font = GameFont.Medium;
            if (Widgets.ButtonText(leftButtonRect, "-", drawBackground: false, overrideTextAnchor: TextAnchor.MiddleCenter)) resources = Math.Max(resources - 1, min);
            if (Widgets.ButtonText(rightButtonRect, "+", drawBackground: false, overrideTextAnchor: TextAnchor.MiddleCenter)) resources = Math.Min(resources + 1, max);
            Text.Font = GameFont.Small;
            Widgets.DrawRectFast(coloredRect, backColor);
            Widgets.Label(backRect, upgradeResource.ToUpgradeStr(resources, true));
        }
    }
}
