using RimElShrine.Data;
using RimElShrine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimElShrine
{
    [Setting]
    public class MechserumSetting
    {
        #region Xml attr classes
        public const string MechserumClass = "mechserum";
        #endregion

        [XmlClassesControl(Class = MechserumClass)]
        [SettingItemBool(Catalog = Constants.CommonCata)]
        [ExposeData]
        public bool UseMechserums = true;

        [SettingItemString(Catalog = Constants.BalanceCata, ParentName = nameof(UseMechserums))]
        [ExposeData] public readonly string MechserumsTitle = "Mechserums";

        [SettingItemInt(1, 240, 1, ParentName = nameof(MechserumsTitle))]
        [ExposeData] public int LimbsRegenSecondsPerHealth = 60;
        [SettingItemFloat(1f, 15f, 0.2f, ParentName = nameof(MechserumsTitle))]
        [ExposeData] public float QuickRegenFactor = 3f;
        [SettingItemFloat(0.0001f, 0.020f, 0.0001f, ValueFormat ="P2", ParentName = nameof(MechserumsTitle))]
        [ExposeData] public float QuickRegenBloodLossPercentPerHealth = 0.005f;
        [SettingItemFloat(0f, 1f, 0.01f, ParentName = nameof(MechserumsTitle))]
        [ExposeData] public float QuickRegenBloodLossMax = 0.35f;
    }
}
