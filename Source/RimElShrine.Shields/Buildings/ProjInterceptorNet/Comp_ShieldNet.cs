using RimWorld;
using Verse;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    public abstract class Comp_ShieldNet : ThingComp
    {
        public virtual ShieldNet? ShieldNet
            => ShieldNet.shieldNets.GetShieldNetByPowerNet(parent.GetComp<CompPower>()?.PowerNet);
        public virtual bool IsNetActive()
            => this.IsPowerActive();

        #region ExtraStr
        public override string CompInspectStringExtra()
        {
            var str = GetNetStr();
            if (!string.IsNullOrEmpty(str)) str += '\n';
            else str = string.Empty;
            str += GetNetDetailsStr();
            return str;
        }
        protected virtual TaggedString GetNetStr()
            => ShieldNet?.GetShieldNetStatus() ?? string.Empty;
        protected virtual TaggedString GetNetDetailsStr()
            => IsNetActive() ? (ShieldNet?.GetUpgradeResourcesStatus() ?? TaggedString.Empty) : "ShieldNet_CompIsShutDown".ES_Translate();
        #endregion

        public T DisableGizmoByShieldNet<T>(T gizmo) where T : Gizmo
        {
            if (!IsNetActive()) gizmo.Disable("ShieldNet_CompIsShutDown".ES_Translate());
            if (ShieldNet == null) gizmo.Disable("ShieldNet_NetIsInvalid".ES_Translate());
            return gizmo;
        }
    }
}
