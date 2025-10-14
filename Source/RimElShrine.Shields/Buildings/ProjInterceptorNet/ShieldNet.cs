using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    [StaticConstructorOnStartup]
    public class ShieldNet
    {
        #region Shield net database

        public readonly static ShieldNetDatabase shieldNets = [];

        public class ShieldNetDatabase : Dictionary<PowerNet, ShieldNet>
        {
            //Automatically remove all empty shield net;
            public ShieldNet? GetShieldNetByPowerNet(PowerNet? powerNet)
            {
                if (powerNet == null) return null;
                if (!TryGetValue(powerNet, out var shieldNet))
                {
                    ClearEmptyShieldNet();
                    shieldNet = new ShieldNet(powerNet);
                    Add(powerNet, shieldNet);
                }
                return shieldNet;
            }
            public bool IsPowerNetEmpty(PowerNet powerNet)
                => powerNet.connectors.Count + powerNet.transmitters.Count < 1;
            public void ClearEmptyShieldNet() => this.RemoveAll(kp => IsPowerNetEmpty(kp.Key));
        }
        #endregion

        public readonly PowerNet powerNet;
        public IReadOnlyList<Comp_ShieldNet> netComps = [];
        public IReadOnlyList<Comp_ShieldNet_ProjInterceptorELS> shields = [];
        public IReadOnlyList<Comp_ShieldNet_ServerUnit> serverUnits = [];
        public IReadOnlyList<Comp_ShieldNet_Terminal> terminals = [];

        public IEnumerable<Comp_ShieldNet_ServerUnit> ActiveServerUnits => serverUnits.Where(su => su.IsNetActive());
        public int Resources => ActiveServerUnits.Sum(s => s.Resources);
        public int AllocatedResources => ActiveServerUnits.Sum(s => s.AllocatedResources);
        public int AvailableResources => ActiveServerUnits.Sum(s => s.AvailabelResources);

        public ShieldNet(PowerNet powerNet)
        {
            this.powerNet = powerNet;
            UpdateShieldNet();
        }
        public virtual void UpdateShieldNet()
        {
            if (powerNet != null)
            {
                var netComps = new List<Comp_ShieldNet>();
                var shields = new List<Comp_ShieldNet_ProjInterceptorELS>();
                var serverUnits = new List<Comp_ShieldNet_ServerUnit>();
                var terminals = new List<Comp_ShieldNet_Terminal>();
                var powerComps = new List<CompPower>();
                powerComps.AddRange(powerNet.connectors);
                powerComps.AddRange(powerNet.transmitters);
                foreach (var comp in powerComps)
                {
                    var compShieldNet = comp.parent.GetComp<Comp_ShieldNet>();
                    if (compShieldNet != null)
                    {
                        netComps.Add(compShieldNet);
                        if (compShieldNet is Comp_ShieldNet_ProjInterceptorELS shield) shields.Add(shield);
                        else if (compShieldNet is Comp_ShieldNet_ServerUnit serverUnit) serverUnits.Add(serverUnit);
                        else if (compShieldNet is Comp_ShieldNet_Terminal terminal) terminals.Add(terminal);
                    }
                }
                this.netComps = netComps;
                this.shields = shields;
                this.serverUnits = serverUnits;
                this.terminals = terminals;
            }
        }

        #region Commands
        public void ResetAllocatedResources()
        {
            foreach (var serverUnit in ActiveServerUnits)
            {
                serverUnit.ResetResources();
            }
        }
        public bool TryAllocateResources(ShieldUpgradeResource upgradeResource, int resourcesToAllocate, out int remained)
        {
            int originalToAllocate = resourcesToAllocate;
            if (resourcesToAllocate > 0)
            {
                foreach (var serverUnit in ActiveServerUnits)
                {
                    if (resourcesToAllocate == 0) break;
                    serverUnit.TryAllocateResource(upgradeResource, resourcesToAllocate, out var left);
                    resourcesToAllocate = left;
                }
            }
            remained = resourcesToAllocate;
            return originalToAllocate > 0 && remained == 0;
        }
        #endregion

        #region Get UpgradeResources and UpgradeValue
        public int GetUpgradeResources(ShieldUpgradeResource upgradeResource)
            => ActiveServerUnits.Sum(s => s.GetUpgradeResources(upgradeResource));
        public float GetUpgradeValue(ShieldUpgradeResource upgradeResource)
            => terminals.Where(t => t.IsPowerActive()).Any() ? upgradeResource.ToUpgradeValue(GetUpgradeResources(upgradeResource)) : 0;
        #endregion

        #region Get Str
        public virtual TaggedString GetShieldNetStatus()
            => "ShieldNet_ContainedShieldNetComps".ES_Translate(shields.Count, serverUnits.Count, terminals.Count, netComps.Where(c => !c.IsNetActive()).Count());

        public virtual TaggedString GetUpgradeResourcesStatus(bool showDetailsZeroItem = false)
        {
            var str = "ShieldNet_ResourcesSummary".ES_Translate(AllocatedResources, Resources);
            var detailStr = $"\n{"ShieldNet_ResourcesDetails".ES_Translate()}:";
            var modified = false;
            foreach (ShieldUpgradeResource upgradeResource in ShieldUpgradeResourceHelper.Enums)
            {
                var ur = GetUpgradeResources(upgradeResource);
                if (showDetailsZeroItem || ur != 0)
                {
                    detailStr += $"\n{upgradeResource.ToUpgradeStr(ur)}";
                    modified = true;
                }
            }
            if (!modified) detailStr = string.Empty;
            return str + detailStr;
        }
        #endregion
    }
}
