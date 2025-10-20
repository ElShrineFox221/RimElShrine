using RimElShrine.Buildings.Shuttle;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimElShrine.Traders.EmergencyChemfuelTrader
{
    public class Dialog_SelectEmgChemfuelTrader(Building_PassengerShuttle shuttle, Pawn negotiator) : Window
    {
        public Caravan? Caravan { get; set; } = shuttle.GetCaravan();
        public PlanetTile? PlanetTile { get; } = shuttle.GetPlanetTile();
        public Building_PassengerShuttle Shuttle { get; set; } = shuttle;
        public Pawn Negotiator { get; set; } = negotiator;
        public override Vector2 InitialSize => new(720f, 600f);
        public override void DoWindowContents(Rect inRect)
        {
            Widgets.BeginGroup(inRect);
            Rect rect = new Rect(0f, 0f, inRect.width / 2f, 70f);
            Rect rect2 = new Rect(0f, rect.yMax, rect.width, 60f);
            Rect rect3 = new Rect(inRect.width / 2f, 0f, inRect.width / 2f, 70f);
            Rect rect4 = new Rect(inRect.width / 2f, rect.yMax, rect.width, 60f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, Negotiator.LabelCap);
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rect3, "SocialSkillIs".Translate(Negotiator.skills.GetSkill(SkillDefOf.Social).Level));
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            Widgets.Label(rect2, "CurrentFuelTradersListNotice".ES_Translate());

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Widgets.EndGroup();
            float num = 117f;
            Rect rect5 = new(0f, num, inRect.width, inRect.height - num);
            DrawFuelTraders(rect5);
        }
        private void DrawFuelTraders(Rect rect)
        {
            const float splitHeight = 30f;
            if (!PlanetTile.HasValue) return;
            var list = new Listing_Standard();
            list.Begin(rect);
            foreach(var trader in EmergencyChemfuelTradeUtility.GetTradersByPlanetTile(PlanetTile.Value))
            {
                var faction = trader.Faction;
                var relationKind = GetFactionRelationKind(faction);
                var text = $"{relationKind.GetLabelCap().Colorize(relationKind.GetColor())}: {trader.TraderName}";
                var height = Text.CalcHeight(text, rect.width);
                var btnRect = list.GetRect(Mathf.Max(height, splitHeight) + Text.SpaceBetweenLines);
                if(Widgets.ButtonText(btnRect, text, false))
                {
                    var tradeWindow = new Dialog_Trade(Negotiator, trader, false);
                    Find.WindowStack.Add(tradeWindow);
                }
            }
            list.End();
            var lastRect = new Rect(rect.x, rect.y + rect.height - splitHeight - Text.SpaceBetweenLines, rect.width, splitHeight);
            var comp = Shuttle.TryGetComp<Comp_AutoRefuelShuttle>();
            var cooldownTicks = comp.emgTradeCooldownTicks;
            var lastText = "SendNewEmgChemfuelTrader".ES_Translate();
            if (cooldownTicks > 0) lastText += $"({"CommandLaunchGroupCooldown".Translate()} {cooldownTicks.ToStringTicksToPeriod()})";
            if (Widgets.ButtonText(lastRect, lastText, false, active: cooldownTicks <= 0)) 
            {
                var trader = EmergencyChemfuelTradeUtility.GetNewEmgChemfuelTrader(Shuttle, Negotiator);
                if(trader is null) Messages.Message($"FoundNoEmgFuelTrader".ES_Translate(), MessageTypeDefOf.RejectInput);
                comp.emgTradeCooldownTicks = Settings.GetSetting<ShuttleFuelSetting>().EmgTradeCooldownSeconds.SecondsToTicks();
            }
        }
        private FactionRelationKind GetFactionRelationKind(Faction faction)
        {
            FactionRelationKind playerRelationKind = faction.RelationWith(Negotiator.Faction, true)?.kind ?? FactionRelationKind.Neutral;
            return playerRelationKind;
        }
    }
}
