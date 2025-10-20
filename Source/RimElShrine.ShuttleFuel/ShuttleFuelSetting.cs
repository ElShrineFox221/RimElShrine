using RimElShrine.Data;
using RimElShrine.UI;

namespace RimElShrine
{
    [Setting]
    public class ShuttleFuelSetting
    {
        [SettingItemString(Catalog = Constants.BalanceCata)]
        [ExposeData] public string ShuttleFuelTitle = "ShuttleFuel";

        [SettingItemInt(50, 400, 10, ParentName = nameof(ShuttleFuelTitle))]
        [ExposeData] public int EmgTradeThresholdInt = 70;
        [SettingItemFloat(0.01f, 1f, 0.01f, ParentName = nameof(ShuttleFuelTitle))]
        [ExposeData] public float EmgTradeThreshold = 0.1f;

        [SettingItemFloat(-1, 1, 0.05f, ParentName = nameof(ShuttleFuelTitle))]
        [ExposeData] public float EmgTradeWeightOffset = 0f;

        [SettingItemInt(-100, 100, 1, ParentName = nameof(ShuttleFuelTitle))]
        [ExposeData] public int EmgTraderPriceGoodwillOffset = 0;

        [SettingItemFloat(0f, 5f, 0.05f, ParentName = nameof(ShuttleFuelTitle))]
        [ExposeData] public float EmgTradePriceDistanceFactor = 1f;

        [SettingItemFloat(1f, 300f, 1f, ValueFormat = "F0", ParentName = nameof(ShuttleFuelTitle))]
        [ExposeData] public float EmgTradeCooldownSeconds = 60f;
    }
}
