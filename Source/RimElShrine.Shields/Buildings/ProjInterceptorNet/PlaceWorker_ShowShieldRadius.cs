using RimWorld;
using UnityEngine;
using Verse;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    public class PlaceWorker_ShowShieldRadius : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing? thing = null)
        {
            int radius = thing is ThingWithComps thingWithComps && thingWithComps.TryGetComp<Comp_ShieldNet_ProjInterceptorELS>(out var comp) ? comp.Radius : -1;
            if (radius < 0)
            {
                var compProps = def.GetCompProperties<CompProperties_ShieldNet_ProjInterceptorELS>();
                if (compProps != null) radius = (compProps.baseMaxRadius + compProps.baseMinRadius) / 2;
            }
            if (radius > 0)
            {
                Vector3 centre;
                if (thing is null) centre = def.Size.ToVector3() / 2f + center.ToVector3();
                else centre = (center - thing.Position).ToVector3() + thing.TrueCenter();
                GenDraw.DrawCircleOutline(centre, radius);
            }
        }
    }
}
