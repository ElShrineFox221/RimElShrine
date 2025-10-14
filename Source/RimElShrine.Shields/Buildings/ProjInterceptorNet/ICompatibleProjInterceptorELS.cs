using RimElShrine.Compatibility;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    public interface ICompatibleProjInterceptorELS : ICompatible
    {
        public bool InterceptProjectile(Comp_ShieldNet_ProjInterceptorELS source, Thing proj, Vector3 interceptorCentre);
        public float CalcEnergyLoss(Comp_ShieldNet_ProjInterceptorELS source, Thing proj);
        public void DoProjectileImpact(Comp_ShieldNet_ProjInterceptorELS source, Thing proj);
    }
}
