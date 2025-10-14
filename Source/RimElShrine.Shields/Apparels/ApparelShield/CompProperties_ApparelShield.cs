using RimWorld;
using System.Collections.Generic;

namespace RimElShrine.Apparels.ApparelShield
{
    public class CompProperties_ApparelShield : CompProperties_Shield, IStageChangeable
    {
        public CompProperties_ApparelShield()
        {
            compClass = typeof(Comp_ApparelShield);
        }
        public List<ApparelShieldInfo> infos = [];

        public int CurrentIndex { get; private set; } = 0;
        public ApparelShieldInfo? currentIndexInfo = null;
        public ApparelShieldInfo CurrentIndexInfo
        {
            get
            {
                if (CurrentIndex == 0)
                {
                    if (currentIndexInfo != null)
                    {
                        if (infos.Count > 0) infos[0] = currentIndexInfo;
                        else infos.Add(currentIndexInfo);
                    }
                    else
                    {
                        if (infos.Count > 0) currentIndexInfo = infos[0];
                        else
                        {
                            currentIndexInfo = new();
                            infos.Add(currentIndexInfo);
                        }
                    }
                }
                else currentIndexInfo = infos[CurrentIndex];
                return currentIndexInfo;
            }
        }

        public void ChangeState(int index)
        {
            if (index >= infos.Count) CurrentIndex = infos.Count - 1;
            else if (index < 0) CurrentIndex = 0;
            else CurrentIndex = index;
        }
    }
}
