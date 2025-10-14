using CombatExtended;
using RimElShrine.Buildings.ProjInterceptorNet;
using RimElShrine.Reflection;
using RimWorld;
using System.Drawing;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RimElShrine.Compatibility.Buildings.ProjInterceptorNet
{
    [Compatible(typeof(ProjectileCE))]
    public class Comp_ShieldNet_ProjInterceptorELS_CompatibleCE : ICompatibleProjInterceptorELS
    {
        public virtual bool InterceptProjectile(Comp_ShieldNet_ProjInterceptorELS source, Thing projThing, Vector3 interceptorCentre)
        {
            var doIntercept = source.isActive;
            if (doIntercept && projThing is ProjectileCE proj)
            {
                var isGroundProj = !proj.def.projectile.flyOverhead;
                var projInRadius = inRadius(proj.TrueCenter().ToVector2());
                var start = proj.origin;
                var end = proj.Destination;
                var startInRadius = inRadius(start);
                var endInRadius = inRadius(end);
                if (isGroundProj && source.InterceptGround) doIntercept = source.InterceptIn && projInRadius && !startInRadius || !source.AllowOut && !projInRadius && startInRadius;
                else if (!isGroundProj && source.InterceptOverhead) doIntercept = source.InterceptIn && projInRadius && endInRadius || !source.AllowOut && !projInRadius && startInRadius;
                else doIntercept = false;
                if (doIntercept && source.IFFOn)
                {
                    var launcher = proj.launcher;
                    var di0 = launcher?.HostileTo(source.parent.Faction) ?? false;
                    var di1 = launcher?.Faction?.HostileTo(source.parent.Faction) ?? false;
                    doIntercept = di0 || di1;
                }
            }
            else doIntercept = false;
            return doIntercept;
            bool inRadius(Vector2 pos) => pos.InRadiusOf(interceptorCentre.ToVector2(), source.Radius);
        }
        public float CalcEnergyLoss(Comp_ShieldNet_ProjInterceptorELS source, Thing projThing)
        {
            var energyLoss = 0f;
            if (projThing is ProjectileCE proj)
            {
                if (source.Setting.UseKineticEnergy)
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
            if (projThing is ProjectileCE proj && !source.DestroyProj) proj.ImpactSomething();
            else projThing.Destroy();
        }
    }
}
