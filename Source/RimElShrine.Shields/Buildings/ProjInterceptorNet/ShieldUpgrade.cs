using RimElShrine.Data;
using Verse;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    public class ShieldUpgrade : IExposable
    {
        [ExposeData] public int MinRadiusResources = 0;
        [ExposeData] public int MaxRadiusResources = 0;
        [ExposeData] public int RechargeSecondsResources = 0;
        [ExposeData] public int RechargeRateResources = 0;
        [ExposeData] public int MaxEnergyResources = 0;
        [ExposeData] public int ChargeSpeedResources = 0;
        public int this[ShieldUpgradeResource index]
        {
            get => index switch
            {
                ShieldUpgradeResource.MinRadius      => MinRadiusResources,
                ShieldUpgradeResource.MaxRadius      => MaxRadiusResources,
                ShieldUpgradeResource.RechargeSeconds=> RechargeSecondsResources,
                ShieldUpgradeResource.RechargeRate   => RechargeRateResources,
                ShieldUpgradeResource.MaxEnergy      => MaxEnergyResources,
                ShieldUpgradeResource.ChargeSpeed    => ChargeSpeedResources,
                _                                   => 0
            };
            set
            {
                switch (index)
                {
                    case ShieldUpgradeResource.MinRadius:
                        MinRadiusResources = value;
                        break;
                    case ShieldUpgradeResource.MaxRadius:
                        MaxRadiusResources = value;
                        break;
                    case ShieldUpgradeResource.RechargeSeconds:
                        RechargeSecondsResources = value;
                        break;
                    case ShieldUpgradeResource.RechargeRate:
                        RechargeRateResources = value;
                        break;
                    case ShieldUpgradeResource.MaxEnergy:
                        MaxEnergyResources = value;
                        break;
                    case ShieldUpgradeResource.ChargeSpeed:
                        ChargeSpeedResources = value;
                        break;
                }
            }
        }
        public void ClearResources()
        {
            MinRadiusResources = 0;
            MaxRadiusResources = 0;
            RechargeSecondsResources = 0;
            RechargeRateResources = 0;
            MaxEnergyResources = 0;
            ChargeSpeedResources = 0;
        }
        public int Resources => MinRadiusResources + MaxRadiusResources + RechargeSecondsResources + RechargeRateResources + MaxEnergyResources + ChargeSpeedResources;
        public void ExposeData()
            => this.ExposeAttributedData();
    }
}
