using RimElShrine.Reflection;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RimElShrine.Apparels.ApparelShield
{
    [StaticConstructorOnStartup]
    public class Comp_ApparelShield : CompShield
    {
        public Comp_ApparelShield() => props = new CompProperties_ApparelShield();
        public Comp_ApparelShield(CompProperties_ApparelShield props) => this.props = props;
        public ShieldSetting Setting => Settings.GetSetting<ShieldSetting>();

        public new CompProperties_ApparelShield Props => (CompProperties_ApparelShield)props;
        public float EnergyMax => Props.CurrentIndexInfo.overrideMaxEnergy > 0 ? Props.CurrentIndexInfo.overrideMaxEnergy : parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax);
        public float EnergyGainPerTick => (Props.CurrentIndexInfo.overrideEnergyGainPerSecond > 0 ? Props.CurrentIndexInfo.overrideEnergyGainPerSecond : parent.GetStatValue(StatDefOf.EnergyShieldRechargeRate)) / 60f;

        protected readonly static List<DamageDef> EnviroDmgDefs 
            = [DamageDefOf.AcidBurn, DamageDefOf.Flame, DamageDefOf.ToxGas, DamageDefOf.TornadoScratch];

        public float CurrentEnergy { get; protected set; } = 0f;

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            var energyLoss = dinfo.Amount * Props.energyLossPerDamage * Setting.DmgGlobalFactor;
            if (ShieldState == ShieldState.Active && PawnOwner != null) 
            {
                if (dinfo.Def == DamageDefOf.EMP)
                {
                    energyLoss *= Setting.DmgEmpFactor * Props.CurrentIndexInfo.absorbEMPFactor;
                    if (Props.CurrentIndexInfo.absorbEMP) AbsorbDamage(dinfo, energyLoss, out absorbed);
                    else
                    {
                        Energy = 0f;
                        Break();
                    }
                }
                else if (EnviroDmgDefs.Contains(dinfo.Def) && Props.CurrentIndexInfo.absorbEnviromental)
                {
                    energyLoss *= Setting.DmgEnviromentalFactor * Props.CurrentIndexInfo.absorbEnviromentalFactor;
                    AbsorbDamage(dinfo, energyLoss, out absorbed);
                }
                else if (!dinfo.Def.isRanged && !dinfo.Def.isExplosive && Props.CurrentIndexInfo.absorbMelee)
                {
                    energyLoss *= Props.CurrentIndexInfo.absorbMeleeFactor * Setting.DmgMeleeFactor;
                    AbsorbDamage(dinfo, energyLoss, out absorbed);
                }
                else if (dinfo.Def.isRanged || dinfo.Def.isExplosive) AbsorbDamage(dinfo, energyLoss, out absorbed);
                else
                {
                    energyLoss *= Setting.DmgOtherFactor;
                    AbsorbDamage(dinfo, energyLoss, out absorbed);
                }
            }
        }
        
        public override void CompTick()
        {
            if (PawnOwner != null)
            {
                if (ShieldState == ShieldState.Resetting)
                {
                    if (Props.CurrentIndexInfo.useBloodTransfer)
                    {
                        var got = PawnOwner.TryGetHediff(HediffDefOf.BloodLoss, out var hed);
                        if (hed is Hediff bloodLossHediff && bloodLossHediff.CurStageIndex < 2)
                        {
                            bloodLossHediff.Severity += Props.CurrentIndexInfo.bloodTransferMount;
                            TicksToReset = 0;
                            Reset();
                            Energy += Props.CurrentIndexInfo.bloodTransferMount * Props.CurrentIndexInfo.energyPerBlood * Setting.BloodEnergyTransferFactor;
                        }
                        else _basicResetTicking();
                    }
                    else _basicResetTicking();
                }
                else if (ShieldState == ShieldState.Active)
                {
                    Energy += EnergyGainPerTick;
                    if (Energy > EnergyMax)
                    {
                        Energy = EnergyMax;
                    }
                }
            }
            void _basicResetTicking()
            {
                TicksToReset--;
                if (TicksToReset <= 0) Reset();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
            => ModifyGizmos(base.CompGetGizmosExtra());
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
            => ModifyGizmos(base.CompGetWornGizmosExtra());
        protected virtual IEnumerable<Gizmo> ModifyGizmos(IEnumerable<Gizmo> originalGizmos)
        {
            foreach(Gizmo gizmo in originalGizmos)
            {
                if (gizmo is Gizmo_EnergyShieldStatus) yield return new Gizmo_ApparelShieldStatus(Props.CurrentIndexInfo.drawingColor) { shield = this };
                else yield return gizmo;
            }
            yield break;
        }

        #region Base Methods
        protected void Break()
        {
            typeof(CompShield).GetCachedMethodInfo(nameof(Break))?.Invoke(this, null);
            EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, 0.7f);
            if (Props.CurrentIndexInfo.overrideTicksToReset > 0) TicksToReset = Props.CurrentIndexInfo.overrideTicksToReset;
        }
        protected void AbsorbedDamage(DamageInfo dinfo) 
            => typeof(CompShield).GetCachedMethodInfo(nameof(AbsorbedDamage))?.Invoke(this, [dinfo]);
        protected void Reset() => typeof(CompShield).GetCachedMethodInfo(nameof(Reset))?.Invoke(this, null);
        #endregion

        protected void AbsorbDamage(DamageInfo dinfo, float amount, out bool absorbed)
        {
            if(Energy > 0)
            {
                Energy -= amount;
                if (Energy < 0f) Break();
                else AbsorbedDamage(dinfo);
                absorbed = true;
            }
            else absorbed = false;
        }

        public new float Energy
        {
            get => energy;
            set => energy = value;
        }
        public int TicksToReset
        {
            get => ticksToReset;
            set => ticksToReset = value;
        }
    }
}
