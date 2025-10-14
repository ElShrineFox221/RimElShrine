using System;
using Verse;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    [StaticConstructorOnStartup]
    public static class ShieldUpgradeResourceHelper
    {
        public readonly static Array Enums = Enum.GetValues(typeof(ShieldUpgradeResource));
        public static float ToUpgradeValue(this ShieldUpgradeResource upgradeResource, int resources)
        {
            var upgradeValue = (float)resources;
            switch (upgradeResource)
            {
                case ShieldUpgradeResource.MinRadius:
                    upgradeValue *= Settings.GetSetting<ShieldSetting>().ResourceToMinRadius;
                    break;
                case ShieldUpgradeResource.MaxRadius:
                    upgradeValue *= Settings.GetSetting<ShieldSetting>().ResourceToMaxRadius;
                    break;
                case ShieldUpgradeResource.RechargeSeconds:
                    upgradeValue *= Settings.GetSetting<ShieldSetting>().ResourceToRechargeSeconds;
                    break;
                case ShieldUpgradeResource.RechargeRate:
                    upgradeValue *= Settings.GetSetting<ShieldSetting>().ResourceToRechargeRate;
                    break;
                case ShieldUpgradeResource.MaxEnergy:
                    upgradeValue *= Settings.GetSetting<ShieldSetting>().ResourceToMaxEnergy;
                    break;
                case ShieldUpgradeResource.ChargeSpeed:
                    upgradeValue *= Settings.GetSetting<ShieldSetting>().ResourceToChargeSpeed;
                    break;
            }
            return upgradeValue;
        }
        public static TaggedString ToUpgradeStr(this ShieldUpgradeResource upgradeResource, int resources, bool showResouces = false)
        {
            var upgradeValue = upgradeResource.ToUpgradeValue(resources);
            string upgradeValueStr;
            if (upgradeResource is ShieldUpgradeResource.RechargeRate || upgradeResource is ShieldUpgradeResource.ChargeSpeed) upgradeValue *= 100;
            if (upgradeValue >= 0) upgradeValueStr = $"+{upgradeValue}";
            else upgradeValueStr = upgradeValue.ToString();
            var upgradeResourcesStr = string.Empty;
            if (showResouces)
            {
                if (resources >= 0) upgradeResourcesStr = $"+{resources}, ";
                else upgradeResourcesStr = resources.ToString() + ", ";
            }
            var str = upgradeResource.ToString().ES_Translate(upgradeValueStr, upgradeResourcesStr);
            return str;
        }
    }
}
