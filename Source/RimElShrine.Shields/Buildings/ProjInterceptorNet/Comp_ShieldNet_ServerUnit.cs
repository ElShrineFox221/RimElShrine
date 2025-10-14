using RimElShrine.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    #region ServerUnit
    public class CompProperties_ShieldNet_ServerUnit : CompProperties
    {
        public int upgradeResources = 3;
        public bool showDetailsZeroItem = false;
        public CompProperties_ShieldNet_ServerUnit() => compClass = typeof(Comp_ShieldNet_ServerUnit);
    }
    [StaticConstructorOnStartup]
    public class Comp_ShieldNet_ServerUnit : Comp_ShieldNet
    {
        public CompProperties_ShieldNet_ServerUnit Props => (CompProperties_ShieldNet_ServerUnit)props;
        [ExposeData] private ShieldUpgrade? upgrade = null;
        public ShieldUpgrade Upgrade => upgrade ??= new ShieldUpgrade();
        public int AllocatedResources => Upgrade.Resources;
        public int Resources => Props.upgradeResources;
        public int AvailabelResources => Resources - AllocatedResources;

        public virtual bool TryAllocateResource(ShieldUpgradeResource upgradeResource, int resourcesToAllocate, out int remainedResources)
        {
            remainedResources = resourcesToAllocate;
            if (resourcesToAllocate <= 0 || AvailabelResources <= 0) return resourcesToAllocate <= 0;

            int canAllocate = Math.Min(resourcesToAllocate, AvailabelResources);
            Upgrade[upgradeResource] += canAllocate;
            remainedResources -= canAllocate;

            Notify_ResourceAllocated(resourcesToAllocate, remainedResources, upgradeResource);
            return remainedResources == 0;
        }
        public virtual void ResetResources() => Upgrade.ClearResources();
        protected virtual void Notify_ResourceAllocated(int resouces, int remain, ShieldUpgradeResource upgradeResource)
        {

        }
        public virtual int GetUpgradeResources(ShieldUpgradeResource upgradeResource) 
            => IsNetActive() ? Upgrade[upgradeResource] : 0;
        public override void PostExposeData() => this.ExposeAttributedData();

        private static Texture2D? resetTx2D;
        public static Texture2D ResetTx2D => resetTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldServerUnit_ResetUpgradeResources");
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var item in base.CompGetGizmosExtra()) yield return item;
            var command_Action_ResetResources = new Command_Action()
            {
                defaultLabel = "ShieldNet_Command_ResetAllocatedResouces_Label".ES_Translate(),
                defaultDesc = "ShieldNet_Command_ResetAllocatedResouces_Desc".ES_Translate(),
                action = ResetResources,
                icon = ResetTx2D,
            };
            if (AllocatedResources <= 0) command_Action_ResetResources.Disable("ShieldNet_Command_ResetAllocatedResouces_DisabledReason".ES_Translate());
            yield return DisableGizmoByShieldNet(command_Action_ResetResources).DisabledByCompFaction(parent.Faction);
        }
        public override string CompInspectStringExtra()
        {
            var str = string.Empty;
            if (ShieldNet is not null)
            {
                str = ShieldNet.GetShieldNetStatus();
                str += $"\n{"ShieldNet_ServerUnit_ResourcesSummary".ES_Translate(AllocatedResources, Resources)}";
                var detailStr = $"\n{"ShieldNet_ResourcesDetails".ES_Translate()}:";
                var modified = false;
                foreach (ShieldUpgradeResource upgradeResource in ShieldUpgradeResourceHelper.Enums)
                {
                    var ur = GetUpgradeResources(upgradeResource);
                    if (Props.showDetailsZeroItem || ur != 0)
                    {
                        detailStr += $"\n{upgradeResource.ToUpgradeStr(ur)}";
                        modified = true;
                    }
                }
                if (modified) str += detailStr;
            }
            return str;
        }
    }
    #endregion
}
