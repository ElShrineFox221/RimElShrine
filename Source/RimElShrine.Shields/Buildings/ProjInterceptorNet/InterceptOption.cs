using RimElShrine.Data;
using Verse;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    public class InterceptOption : IExposable, ICloneable<InterceptOption>
    {
        [ExposeData] public bool IFFOn = true;
        [ExposeData] public bool AllowOut = true;
        [ExposeData] public bool InterceptIn = true;
        [ExposeData] public bool InterceptOverhead = true;
        [ExposeData] public bool InterceptGround = true;
        [ExposeData] public bool DestoryProjectile = false;

        public InterceptOption Clone()
            => new()
            {
                IFFOn = IFFOn,
                AllowOut = AllowOut,
                InterceptIn = InterceptIn,
                InterceptOverhead = InterceptOverhead,
                InterceptGround = InterceptGround,
                DestoryProjectile = DestoryProjectile
            };

        public void ExposeData() => this.ExposeAttributedData();
    }
}
