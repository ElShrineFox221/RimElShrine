using RimElShrine.Data;
using RimElShrine.UI;

namespace RimElShrine
{
    [Setting]
    public class ShieldSetting
    {
        #region Xml attr classes
        public const string ShieldApparelClass = "shield_apparel";
        public const string ShieldBuildingClass = "shield_building";
        public const string ShieldNetworkClass = "shield_network";
        #endregion

        #region ShieldApparel
        [XmlClassesControl(Class = ShieldApparelClass)]
        [SettingItemBool(Catalog = Constants.CommonCata)]
        [ExposeData]
        public bool UseShieldApparels = true;

        [SettingItemString(ParentName = nameof(UseShieldApparels))]
        [ExposeData]
        public readonly string ShieldApparelTitle = "ShieldApparel";

        [SettingItemFloat(0.1f, 5f, 0.1f, Catalog = Constants.BalanceCata, ParentName = nameof(ShieldApparelTitle))]
        [ExposeData]
        public float DmgGlobalFactor = 1.0f;
        [SettingItemFloat(0f, 20f, 0.1f, ParentName = nameof(DmgGlobalFactor))]
        [ExposeData]
        public float DmgEmpFactor = 3f;
        [SettingItemFloat(0f, 20f, 0.1f, ParentName = nameof(DmgGlobalFactor))]
        [ExposeData]
        public float DmgMeleeFactor = 5f;
        [SettingItemFloat(0f, 20f, 0.1f, ParentName = nameof(DmgGlobalFactor))]
        [ExposeData]
        public float DmgEnviromentalFactor = 10f;
        [SettingItemFloat(0f, 20f, 0.1f, ParentName = nameof(DmgGlobalFactor))]
        [ExposeData]
        public float DmgOtherFactor = 1.0f;
        [SettingItemFloat(0f, 10f, 0.2f, ParentName = nameof(ShieldApparelTitle))]
        [ExposeData]
        public float BloodEnergyTransferFactor = 1f;
        #endregion

        #region ShieldBuilding

        [XmlClassesControl(Class = ShieldBuildingClass)]
        [SettingItemBool(Catalog = Constants.CommonCata)]
        [ExposeData] public bool UseShieldBuilding = true;
        [XmlClassesControl(Class = ShieldNetworkClass)]
        [SettingItemBool(ParentName = nameof(UseShieldBuilding))]
        [ExposeData] public bool UseShieldNetwork = true;

        [SettingItemString(ParentName = nameof(UseShieldBuilding))]
        [ExposeData] public readonly string ShieldBuildingTitle = "ShieldBuilding";
        [SettingItemString(ParentName = nameof(UseShieldNetwork))]
        [ExposeData] public readonly string ShieldNetTitle = "ShieldNet";

        [SettingItemFloat(0.1f, 10f, 0.1f, ParentName = nameof(ShieldBuildingTitle), Catalog = Constants.BalanceCata)]
        [ExposeData] public float BuildingDmgGlobalFactor = 1f;
        [SettingItemInt(-15, 30, 1, ParentName = nameof(ShieldBuildingTitle))]
        [ExposeData] public int MinRadiusOffset = 0;
        [SettingItemInt(-15, 30, 1, ParentName = nameof(ShieldBuildingTitle))]
        [ExposeData] public int MaxRadiusOffset = 0;
        [SettingItemFloat(0.1f, 5f, 0.1f, ParentName = nameof(ShieldBuildingTitle))]
        [ExposeData] public float RechargeTimeFactor = 1f;
        [SettingItemFloat(0.1f, 5f, 0.1f, ParentName = nameof(ShieldBuildingTitle))]
        [ExposeData] public float RechargeRateFactor = 1f;
        [SettingItemFloat(0.1f, 5f, 0.1f, ParentName = nameof(ShieldBuildingTitle))]
        [ExposeData] public float MaxEnergyFactor = 1f;
        [SettingItemFloat(0.1f, 10f, 0.1f, ParentName = nameof(ShieldBuildingTitle))]
        [ExposeData] public float ChargeSpeed = 1f;
        [SettingItemFloat(0.1f, 5f, 0.1f, ParentName = nameof(ShieldBuildingTitle))]
        [ExposeData] public float DisarmedByEmpTimeFactor = 1f;
        [SettingItemBool(ParentName = nameof(ShieldBuildingTitle))]
        [ExposeData] public bool UseKineticEnergy = false;
        [SettingItemFloat(0, 1, 0.01f, ParentName = nameof(UseKineticEnergy))]
        [ExposeData] public float NormalKineticEnergyFactor = 0.2f;
        [SettingItemFloat(0, 10, 0.1f, ParentName = nameof(UseKineticEnergy))]
        [ExposeData] public float MortarKineticEnergyFactor = 1f;

        [SettingItemFloat(0.02f, 1f, 0.02f, ParentName = nameof(ShieldNetTitle))]
        [ExposeData] public float SlowChargeFactor = 0.1f;
        [SettingItemInt(-5, -1, 1, ParentName = nameof(ShieldNetTitle))]
        [ExposeData] public int ResourceToMinRadius = -1;
        [SettingItemInt(1, 5, 1, ParentName = nameof(ShieldNetTitle))]
        [ExposeData] public int ResourceToMaxRadius = 1;
        [SettingItemFloat(-10, -1, 0.2f, ValueFormat = "F0", ParentName = nameof(ShieldNetTitle))]
        [ExposeData] public float ResourceToRechargeSeconds = -2f;
        [SettingItemFloat(0.05f, 1, 0.05f, ParentName = nameof(ShieldNetTitle))]
        [ExposeData] public float ResourceToRechargeRate = 0.2f;
        [SettingItemInt(100, 1000, 20, ParentName = nameof(ShieldNetTitle))]
        [ExposeData] public int ResourceToMaxEnergy = 200;
        [SettingItemFloat(0.02f, 1, 0.02f, ParentName = nameof(ShieldNetTitle))]
        [ExposeData] public float ResourceToChargeSpeed = 0.1f;

        [SettingItemFloat(1, 3, 0.2f, Catalog = Constants.RenderCatalog, ParentName = nameof(ShieldBuildingTitle))]
        [ExposeData] public float RenderAlphaFactor = 2f;
        [SettingItemFloat(0.5f, 5, 0.1f, Catalog = Constants.RenderCatalog, ParentName = nameof(ShieldBuildingTitle))]
        [ExposeData] public float HitEffectLastFactor = 1f;
        #endregion
    }
}
