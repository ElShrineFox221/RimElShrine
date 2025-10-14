using RimElShrine.Compatibility;
using RimElShrine.Data;
using RimElShrine.Reflection;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    #region Shield
    public class CompProperties_ShieldNet_ProjInterceptorELS : CompProperties
    {
        //Range options
        public int baseMinRadius = 4;
        public int baseMaxRadius = 20;

        //Gizmo & Intercept options
        public InterceptOption defaultInterceptOpt = new();
        public bool canConfigureIFFOn = true;
        public bool canConfigureAllowOut = true;
        public bool canConfigureInterceptIn = true;
        public bool canConfigureInterceptOverhead = true;
        public bool canConfigureInterceptGround = true;
        public bool canConfigureRadius = true;
        public bool canConfigureDestroyProj = true;

        //energy options
        public bool isInfiniteEnergy = true;
        public float baseMaxEnergy = 1000;
        public float baseChargePerSecond = 60f;
        public bool enabledSlowCharge = true;
        public float slowChargeExtraFactor = 1f;
        public float baseRechargeSeconds = 20f;
        public float baseRechargeRate = 0.1f;
        public float baseDestroyProjComsumeFactor = 2f;
        public float disarmedSecondsByEmp = 5f;
        
        //Render options
        public Color color = new(0.2f, 0.2f, 0.2f);
        public float alpha = 0.7f;
        public float selectionAlphaAddtional = 0.5f;
        public float interceptAlphaAdditional = 1f;
        public float alphaRandomLength = 0.4f;

        //ShieldUpgrade
        public int MaxUpgrades = 10;
        public CompProperties_ShieldNet_ProjInterceptorELS() => compClass = typeof(Comp_ShieldNet_ProjInterceptorELS);
    }

    [StaticConstructorOnStartup]
    [Compatible(typeof(Projectile))]
    public class Comp_ShieldNet_ProjInterceptorELS : Comp_ShieldNet, ICompatibleProjInterceptorELS
    {
        public CompProperties_ShieldNet_ProjInterceptorELS Props => (CompProperties_ShieldNet_ProjInterceptorELS)props;
        public ShieldSetting Setting => Settings.GetSetting<ShieldSetting>();

        #region Default from props && variables

        #region Range
        public int MinRadius => Math.Max(Props.baseMinRadius + Mathf.RoundToInt(ShieldNet?.GetUpgradeValue(ShieldUpgradeResource.MinRadius) ?? 0) + Setting.MinRadiusOffset, 1);
        public int MaxRadius => Math.Max(Props.baseMaxRadius + Mathf.RoundToInt(ShieldNet?.GetUpgradeValue(ShieldUpgradeResource.MaxRadius) ?? 0) + Setting.MaxRadiusOffset, 1);
        [ExposeData] private int radius = -1;
        public int Radius
        {
            get
            {
                if (radius < 0) radius = (MaxRadius + MinRadius) / 2;
                return radius;
            }
            set => radius = Mathf.Clamp(value, Math.Min(MinRadius, MaxRadius), Math.Max(MinRadius, MaxRadius));
        }
        #endregion

        #region Interception
        [ExposeData] private InterceptOption? interception = null;
        public InterceptOption Interception
        {
            get => interception ?? Props.defaultInterceptOpt;
            set => interception = value;
        }
        public bool IFFOn
        {
            get => Interception.IFFOn;
            set => (interception ??= Interception.Clone()).IFFOn = value;
        }
        public bool AllowOut
        {
            get => Interception.AllowOut;
            set => (interception ??= Interception.Clone()).AllowOut = value;
        }
        public bool InterceptIn
        {
            get => Interception.InterceptIn;
            set => (interception ??= Interception.Clone()).InterceptIn = value;
        }
        public bool InterceptOverhead
        {
            get => Interception.InterceptOverhead;
            set => (interception ??= Interception.Clone()).InterceptOverhead = value;
        }
        public bool InterceptGround
        {
            get => Interception.InterceptGround;
            set => (interception ??= Interception.Clone()).InterceptGround = value;
        }
        public bool DestroyProj
        {
            get => Interception.DestoryProjectile;
            set => (interception ??= Interception.Clone()).DestoryProjectile = value;
        }
        #endregion

        #region Energy
        public bool IsInfiniteEnergy
        {
            get => Props.isInfiniteEnergy;
            set => Props.isInfiniteEnergy = value;
        }
        public float MaxEnergy => Mathf.Max((Props.baseMaxEnergy + (ShieldNet?.GetUpgradeValue(ShieldUpgradeResource.MaxEnergy) ?? 0)) * Setting.MaxEnergyFactor, 1);
        public int RechargeAllTicks => Math.Max(Mathf.RoundToInt((Props.baseRechargeSeconds + (ShieldNet?.GetUpgradeValue(ShieldUpgradeResource.RechargeSeconds) ?? 0)).TimeToTicks() * Setting.RechargeTimeFactor), 2);
        public float RechargeRate => Mathf.Max((Props.baseRechargeRate + (ShieldNet?.GetUpgradeValue(ShieldUpgradeResource.RechargeRate) ?? 0)) * Setting.RechargeRateFactor, 0.1f);
        public float ChargePerTick => Mathf.Max(Props.baseChargePerSecond / CommonUtility.TimeToTicks(1f) * (1 + (ShieldNet?.GetUpgradeValue(ShieldUpgradeResource.ChargeSpeed) ?? 0)) * Setting.ChargeSpeed, 0.1f);
        public float SlowChargePerTick => Mathf.Max(ChargePerTick * Props.slowChargeExtraFactor * Setting.SlowChargeFactor, 0.01f);
        #endregion

        #endregion

        #region Variables 
        [ExposeData] public float energy = 0f;
        [ExposeData] public bool isActive = false;
        [ExposeData] public bool isRecharging = false;

        [ExposeData] public int rechargeTotalTicks = 100;
        [ExposeData] public int rechargeTicks = -1;

        protected StunHandler? stunner;
        protected StunHandler Stunner => stunner ??= new StunHandler(parent);
        protected int lastActiveTicks = 0;
        protected class InterceptionInfo
        {
            public InterceptionInfo(float angle, int timeLine = -1, int lastTicks = 100)
            {
                if (timeLine >= 0) CreatedTicks = timeLine;
                else CreatedTicks = Find.TickManager.TicksGame;
                Angle = angle;
                LastTicks = lastTicks;
            }
            public readonly int CreatedTicks = -1;
            public readonly int LastTicks = 50;
            public readonly float Angle = 0;
        }
        protected List<InterceptionInfo> interceptions = [];
        #endregion

        #region Sound and Effect
        private Sustainer? sustainer = null;
        public void SustainSound()
        {
            if (isActive)
            {
                if (sustainer is null || sustainer.Ended) sustainer = LocalDefOf.MortarShield_Ambience.TrySpawnSustainer(SoundInfo.InMap(parent));
                sustainer.Maintain();
            }
            else if (sustainer != null && !sustainer.Ended) sustainer.End();

            if (lastActiveTicks == Find.TickManager.TicksGame) LocalDefOf.MortarShieldGenerator_Reactivate.Spawn(parent, parent.MapHeld).Cleanup();
        }
        #endregion

        #region Tick
        public override void CompTick()
        {
            Stunner.StunHandlerTick();
            if (!Stunner.Stunned)
            {
                var powerOn = IsNetActive();
                if (powerOn)
                {
                    if (isActive)
                    {
                        var slowCharge = Setting.UseShieldNetwork && Props.enabledSlowCharge && (!ShieldNet?.terminals.Where(t => t.IsNetActive()).Any() ?? true);
                        float energyToCharge = slowCharge ? SlowChargePerTick : ChargePerTick;
                        energy = Mathf.Min(energy + energyToCharge, MaxEnergy);
                    }
                    else if (!isRecharging) doRecharge();
                }
                else
                {
                    isRecharging = false;
                    if (energy > 0) energy = Mathf.Max(energy - Mathf.Max(SlowChargePerTick, ChargePerTick), -1);
                }

                if (isRecharging)
                {
                    rechargeTicks--;
                    if (rechargeTicks <= 0)
                    {
                        rechargeTicks = 0;
                        isRecharging = false;
                        isActive = true;
                        energy = RechargeRate * MaxEnergy;
                        lastActiveTicks = Find.TickManager.TicksGame;
                        Notify_RechargeCompleted();
                    }
                }
                if (energy < 0) doRecharge();
                SustainSound();
                DoInterception();
            }
            base.CompTick();
            void doRecharge()
            {
                energy = 0;
                isActive = false;
                isRecharging = true;
                rechargeTotalTicks = RechargeAllTicks;
                rechargeTicks = rechargeTotalTicks;
            }
        }
        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);
        }
        protected virtual void Notify_RechargeCompleted()
        {

        }

        #endregion
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref stunner, "stunner", parent);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && stunner == null) stunner = new StunHandler(parent);
            this.ExposeAttributedData();
        }
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            if (dinfo.Def == DamageDefOf.EMP && isActive)
            {
                energy = 0;
                isActive = false;
                BreakShield(dinfo);
            }
            else Stunner.Notify_DamageApplied(dinfo);
        }

        #region Projectile interception
        protected Map? map = null;

        private void DoInterception()
        {
            map ??= parent.Map;
            if (map != null)
            {
                var projs = map.listerThings.ThingsInGroup(ThingRequestGroup.Projectile).Where(p => p != null).ToArray();
                var centre = parent.TrueCenter();
                var energyLossSum = 0f;
                foreach (var p in projs)
                {
                    var compatibleItem = p.GetType().Assembly.GetCompatibleItem<ICompatibleProjInterceptorELS>();
                    if (compatibleItem?.InterceptProjectile(this, p, centre) ?? false)
                    {
                        var energyLoss = compatibleItem.CalcEnergyLoss(this, p) * Setting.BuildingDmgGlobalFactor;
                        //do hit effect
                        TriggerEffecter(p.Position);
                        var lastTicksP = Math.Min(Math.Max(Mathf.RoundToInt(energyLoss * 2 + 40), 40), 120) * Setting.HitEffectLastFactor;
                        var lastTicks = Math.Min(Math.Max(Mathf.RoundToInt(lastTicksP), 20), 180);
                        interceptions.Add(new(p.TrueCenter().AngleToFlat(parent.TrueCenter()), -1, lastTicks));
                        //sum energy loss
                        if (!IsInfiniteEnergy) energyLossSum += energyLoss;
                        compatibleItem.DoProjectileImpact(this, p);
                    }
                }
                //remove overtime interception effects
                interceptions.RemoveAll(i => (Find.TickManager.TicksGame - i.LastTicks) > i.CreatedTicks);
                if (energyLossSum > 0)
                {
                    //apply energy loss
                    if (DestroyProj) energyLossSum *= Props.baseDestroyProjComsumeFactor;
                    energy -= energyLossSum;
                    if (energy <= 0)
                    {
                        BreakShield(new DamageInfo(DamageDefOf.Bullet, energyLossSum));
                        isActive = false;
                    }
                }
            }
        }
        public bool InterceptProjectile(Comp_ShieldNet_ProjInterceptorELS source, Thing projThing, Vector3 interceptorCentre)
        {
            var doIntercept = source.isActive;
            if (doIntercept && projThing is Projectile proj)
            {
                var isGroundProj = !proj.def.projectile.flyOverhead;
                var projInRadius = inRadius(proj.TrueCenter());
                var start = (Vector3)proj.GetType().GetCachedFieldInfo("origin").GetValue(proj);
                var end = (Vector3)proj.GetType().GetCachedFieldInfo("destination").GetValue(proj);
                var startInRadius = inRadius(start);
                var endInRadius = inRadius(end);
                if (isGroundProj && source.InterceptGround) doIntercept = source.InterceptIn && projInRadius && !startInRadius || !source.AllowOut && !projInRadius && startInRadius;
                else if (!isGroundProj && source.InterceptOverhead) doIntercept = source.InterceptIn && projInRadius && endInRadius || !source.AllowOut && !projInRadius && startInRadius;
                else doIntercept = false;
                if (doIntercept && source.IFFOn)
                {
                    var launcher = proj.GetType().GetCachedFieldInfo("launcher")?.GetValue(proj) as Thing;
                    var di0 = launcher?.HostileTo(source.parent.Faction) ?? false;
                    var di1 = launcher?.Faction?.HostileTo(source.parent.Faction) ?? false;
                    doIntercept = di0 || di1;
                }
            }
            else doIntercept = false;
            return doIntercept;
            bool inRadius(Vector3 pos) => pos.InRadiusOf(interceptorCentre, source.Radius, true);
        }
        public float CalcEnergyLoss(Comp_ShieldNet_ProjInterceptorELS source, Thing projThing)
        {
            var energyLoss = 0f;
            if (projThing is Projectile proj)
            {
                if (Setting.UseKineticEnergy)
                {
                    var isMortar = proj.def.projectile.flyOverhead;
                    var speed = proj.def.projectile.SpeedTilesPerTick * 60 * (isMortar ? source.Setting.MortarKineticEnergyFactor : source.Setting.NormalKineticEnergyFactor);
                    energyLoss = Mathf.Pow(speed, 2) / 2f * proj.def.BaseMass;
                }
                else energyLoss = proj.DamageAmount;
            }
            return energyLoss;
        }
        public void DoProjectileImpact(Comp_ShieldNet_ProjInterceptorELS source, Thing projThing)
        {
            if(projThing is Projectile proj && !source.DestroyProj) proj.GetType().GetCachedMethodInfo("Impact")?.Invoke(proj, [null, true]);
            else projThing.Destroy();
        }
        private void TriggerEffecter(IntVec3 pos)
        {
            Effecter effecter = new(EffecterDefOf.Interceptor_BlockedProjectile);
            effecter.Trigger(new TargetInfo(pos, parent.Map), TargetInfo.Invalid);
            effecter.Cleanup();
        }
        private void BreakShield(DamageInfo dInfo)
        {
            var isEmp = dInfo.Def == DamageDefOf.EMP;
            if (isEmp)
            {
                float fTheta;
                Vector3 center;
                int num = Mathf.CeilToInt(Radius * 2f);
                fTheta = (float)Math.PI * 2f / num;
                center = parent.TrueCenter();
                for (int i = 0; i < num; i++)
                {
                    FleckMaker.ConnectingLine(PosAtIndex(i), PosAtIndex((i + 1) % num), FleckDefOf.LineEMP, parent.Map, 1.5f);
                }

                dInfo.SetAmount(Settings.GetSetting<ShieldSetting>().DisarmedByEmpTimeFactor * 2);
                Vector3 PosAtIndex(int index)
                {
                    return new Vector3(Radius * Mathf.Cos(fTheta * index) + center.x, 0f, Radius * Mathf.Sin(fTheta * index) + center.z);
                }
            }
            EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, Radius);
            interceptions.Clear();
            Stunner.Notify_DamageApplied(dInfo);
        }
        #endregion
        //Activa Sound and Effect

        #region Visual - Draw shield

        #region Materials
        private static MaterialPropertyBlock? matPropertyBlock;
        private static MaterialPropertyBlock MatPropertyBlock => matPropertyBlock ??= new MaterialPropertyBlock();

        private static Material? forceFieldMat;
        private static Material ForceFieldMat => forceFieldMat ??= MaterialPool.MatFrom("Other/ForceField", ShaderDatabase.MoteGlow);

        private static Material? forceFieldConeMat;
        private static Material ForceFieldConeMat => forceFieldConeMat ??= MaterialPool.MatFrom("Other/ForceFieldCone", ShaderDatabase.MoteGlow);

        private static Material? shieldDotMat;
        private static Material ShieldDotMat => shieldDotMat ??= MaterialPool.MatFrom("Things/Mote/ShieldDownDot", ShaderDatabase.MoteGlow);
        private static readonly Color InactiveColor = new(0.2f, 0.2f, 0.2f);
        #endregion

        public override void PostDraw()
        {
            base.PostDraw();
            Vector3 drawPos = parent.DrawPos;
            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            //
            var alpha = 0f;
            var selected = Find.Selector.IsSelected(parent);
            var halfLength = Props.alphaRandomLength / 2f;
            var interceptBaseAlpha = 0f;
            if (isActive)
            {
                alpha = getAlphaFromBaseLineValue(Props.alpha, halfLength, Find.TickManager.TicksGame);
                interceptBaseAlpha = Props.alpha + Props.interceptAlphaAdditional + halfLength;
                if (selected)
                {
                    alpha += Props.selectionAlphaAddtional;
                    interceptBaseAlpha += Props.selectionAlphaAddtional;
                }
            }
            //
            if (alpha > 0f)
            {
                Color value = !isActive && Find.Selector.IsSelected(parent) ? InactiveColor : Props.color;
                value.a *= alpha;
                MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
                Matrix4x4 matrix = default;
                matrix.SetTRS(drawPos, Quaternion.identity, new Vector3(Radius * 2f * 1.16015625f, 1f, Radius * 2f * 1.16015625f));
                Graphics.DrawMesh(MeshPool.plane10, matrix, ForceFieldMat, 0, null, 0, MatPropertyBlock);
            }
            foreach (var interceptInfo in interceptions)
            {
                Color color = Props.color;
                var interceptAlpha = getAlphaFromBaseLineValue(interceptBaseAlpha, interceptBaseAlpha, Math.Min(Find.TickManager.TicksGame - interceptInfo.CreatedTicks, interceptInfo.LastTicks));
                color.a *= interceptAlpha;
                MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, color);
                Matrix4x4 matrix2 = default;
                matrix2.SetTRS(drawPos, Quaternion.Euler(0f, interceptInfo.Angle - 90f, 0f), new Vector3(Radius * 2f * 1.16015625f, 1f, Radius * 2f * 1.16015625f));
                Graphics.DrawMesh(MeshPool.plane10, matrix2, ForceFieldConeMat, 0, null, 0, MatPropertyBlock);
            }
            static float getAlphaFromBaseLineValue(float baseValue, float hl, int timeLine = 0)
            {
                var low = baseValue - hl;
                var high = baseValue + hl;
                var sin = (Mathf.Sin(timeLine * RandFactor + HalfPI) + 1) / 2f;
                var a = Mathf.Lerp(low, high, sin);
                return a;
            }
        }
        protected virtual int NumInactiveDots => Math.Max(Radius, 6);
        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            if (!isActive && !WorldComponent_GravshipController.GravshipRenderInProgess)
            {
                for (int i = 0; i < NumInactiveDots; i++)
                {
                    Vector3 vector = new Vector3(0f, 0f, 1f).RotatedBy(i / (float)NumInactiveDots * 360f) * (Radius * 0.966f);
                    Vector3 vector2 = parent.DrawPos + vector;
                    Graphics.DrawMesh(MeshPool.plane10, new Vector3(vector2.x, AltitudeLayer.MoteOverhead.AltitudeFor(), vector2.z), Quaternion.identity, ShieldDotMat, 0);
                }
            }
        }
        private const float RandFactor = Mathf.PI / 100f;
        private const float HalfPI = Mathf.PI / 2f;
        #endregion

        #region Gizmos and desc - Status ui
        public override string CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            if (Stunner.Stunned)
            {
                if (stringBuilder.Length != 0) stringBuilder.AppendLine();
                stringBuilder.AppendTagged('\n' + "DisarmedTime".ES_Translate() + ": " + Stunner.StunTicksLeft.ToStringTicksToPeriod());
            }
            return base.CompInspectStringExtra() + stringBuilder.ToString();
        }

        #region Textures
        private static Texture2D? iffOnTx2D;
        private static Texture2D IFFOnTx2D => iffOnTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldIFF");

        private static Texture2D? allowOutTx2D;
        private static Texture2D AllowOutTx2D => allowOutTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldAllowOut");

        private static Texture2D? interceptInTx2D;
        private static Texture2D InterceptInTx2D => interceptInTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldInterceptIn");

        private static Texture2D? interceptOverheadTx2D;
        private static Texture2D InterceptOverheadTx2D => interceptOverheadTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldInterceptOverhead");

        private static Texture2D? interceptGroundTx2D;
        private static Texture2D InterceptGroundTx2D => interceptGroundTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldInterceptGround");

        private static Texture2D? increaseRadiusTx2D;
        private static Texture2D IncreaseRadiusTx2D => increaseRadiusTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldRadiusIncrease");

        private static Texture2D? decreaseRadiusTx2D;
        private static Texture2D DecreaseRadiusTx2D => decreaseRadiusTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldRadiusDecrease");

        private static Texture2D? destroyProjTx2D;
        private static Texture2D DestroyProjTx2D => destroyProjTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldDestroyProj");
        #endregion

        private IEnumerable<Gizmo> GetOptionsGizmos()
        {
            var diabled = !IsNetActive();
            if (Props.canConfigureIFFOn) yield return DisableGizmoByShieldNet(new Command_Toggle()
            {
                defaultLabel = toLabelTranslated(nameof(IFFOn)),
                defaultDesc = toDescTranslated(nameof(IFFOn)),
                icon = IFFOnTx2D,
                isActive = () => IFFOn,
                toggleAction = () => IFFOn = !IFFOn
            }).DisabledByCompFaction(parent.Faction);
            if (Props.canConfigureAllowOut) yield return DisableGizmoByShieldNet(new Command_Toggle()
            {
                defaultLabel = toLabelTranslated(nameof(AllowOut)),
                defaultDesc = toDescTranslated(nameof(AllowOut)),
                icon = AllowOutTx2D,
                isActive = () => AllowOut,
                toggleAction = () => AllowOut = !AllowOut
            }).DisabledByCompFaction(parent.Faction);
            if (Props.canConfigureInterceptIn) yield return DisableGizmoByShieldNet(new Command_Toggle()
            {
                defaultLabel = toLabelTranslated(nameof(InterceptIn)),
                defaultDesc = toDescTranslated(nameof(InterceptIn)),
                icon = InterceptInTx2D,
                isActive = () => InterceptIn,
                toggleAction = () => InterceptIn = !InterceptIn
            }).DisabledByCompFaction(parent.Faction);
            if (Props.canConfigureInterceptOverhead) yield return DisableGizmoByShieldNet(new Command_Toggle()
            {
                defaultLabel = toLabelTranslated(nameof(InterceptOverhead)),
                defaultDesc = toDescTranslated(nameof(InterceptOverhead)),
                icon = InterceptOverheadTx2D,
                isActive = () => InterceptOverhead,
                toggleAction = () => InterceptOverhead = !InterceptOverhead
            }).DisabledByCompFaction(parent.Faction);
            if (Props.canConfigureInterceptGround) yield return DisableGizmoByShieldNet(new Command_Toggle()
            {
                defaultLabel = toLabelTranslated(nameof(InterceptGround)),
                defaultDesc = toDescTranslated(nameof(InterceptGround)),
                icon = InterceptGroundTx2D,
                isActive = () => InterceptGround,
                toggleAction = () => InterceptGround = !InterceptGround
            }).DisabledByCompFaction(parent.Faction);
            if (Props.canConfigureDestroyProj) yield return DisableGizmoByShieldNet(new Command_Toggle()
            {
                defaultLabel = toLabelTranslated(nameof(DestroyProj)),
                defaultDesc = toDescTranslated(nameof(DestroyProj)),
                icon = DestroyProjTx2D,
                isActive = () => DestroyProj,
                toggleAction = () => DestroyProj = !DestroyProj
            }).DisabledByCompFaction(parent.Faction);

            var increaseRadiusCommand = DisableGizmoByShieldNet(new Command_Action()
            {
                defaultLabel = toLabelTranslated("IncreaseRadius"),
                defaultDesc = toDescTranslated("IncreaseRadius"),
                icon = IncreaseRadiusTx2D,
                action = () => Radius++
            }).DisabledByCompFaction(parent.Faction);
            if (Radius >= MaxRadius) increaseRadiusCommand.Disable(toDisabledReasonTranslated("IncreaseRadius"));
            yield return increaseRadiusCommand;
            var decreaseRadiusCommand = DisableGizmoByShieldNet(new Command_Action()
            {
                defaultLabel = toLabelTranslated("DecreaseRadius"),
                defaultDesc = toDescTranslated("DecreaseRadius"),
                icon = DecreaseRadiusTx2D,
                action = () => Radius--
            }).DisabledByCompFaction(parent.Faction);
            if (Radius <= MinRadius) decreaseRadiusCommand.Disable(toDisabledReasonTranslated("DecreaseRadius"));
            yield return decreaseRadiusCommand;

            string toLabelTranslated(string name) => $"ShieldNet_Command_{name}_Label".ES_Translate();
            string toDescTranslated(string name) => $"ShieldNet_Command_{name}_Desc".ES_Translate();
            string toDisabledReasonTranslated(string name) => $"ShieldNet_Command_{name}_DisabledReason".ES_Translate();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var item in base.CompGetGizmosExtra()) yield return item;
            yield return new Gizmo_ELSProjectileInterceptorHitPoints { interceptor = this };
            foreach (var item in GetOptionsGizmos()) yield return item;
        }

        [StaticConstructorOnStartup]
        private class Gizmo_ELSProjectileInterceptorHitPoints : Gizmo
        {
            public Comp_ShieldNet_ProjInterceptorELS? interceptor;
            private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
            private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
            private const float Width = 140f;
            public const int InRectPadding = 6;
            public Gizmo_ELSProjectileInterceptorHitPoints() => Order = -100f;
            public override float GetWidth(float maxWidth) => Width;
            public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
            {
                Rect rect = new(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
                Rect rect2 = rect.ContractedBy(6f);
                Widgets.DrawWindowBackground(rect);

                float percent;
                TaggedString noticeLabel;
                string progressBarLabel;
                if (interceptor is not null && interceptor.isActive)
                {
                    noticeLabel = "ShieldEnergy".ES_Translate();
                    if (interceptor.IsInfiniteEnergy)
                    {
                        percent = 1;
                        progressBarLabel = "InfiniteEnergy".ES_Translate(); // custom label key
                    }
                    else
                    {
                        percent = interceptor.energy / interceptor.MaxEnergy;
                        progressBarLabel = $"{interceptor.energy:F0}/{interceptor.MaxEnergy:F0}";
                    }
                }
                else if (interceptor is not null && interceptor.isRecharging)
                {
                    percent = interceptor.rechargeTicks / interceptor.rechargeTotalTicks;
                    noticeLabel = "ShieldTimeToRecovery".ES_Translate();
                    progressBarLabel = $"{interceptor.rechargeTicks / 60f:F1}";
                }
                else
                {
                    percent = 0;
                    noticeLabel = "ShieldDisabled".ES_Translate(); // custom label key
                    progressBarLabel = "NaN"; // custom label key
                }

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                Rect rect3 = new(rect2.x, rect2.y - 2f, rect2.width, rect2.height / 2f);
                Widgets.Label(rect3, noticeLabel);
                Rect rect4 = new(rect2.x, rect3.yMax, rect2.width, rect2.height / 2f);
                Widgets.FillableBar(rect4, percent, FullBarTex, EmptyBarTex, doBorder: false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect4, progressBarLabel);
                Text.Anchor = TextAnchor.UpperLeft;
                return new GizmoResult(GizmoState.Clear);
            }
        }
        #endregion
    }
    #endregion
}
