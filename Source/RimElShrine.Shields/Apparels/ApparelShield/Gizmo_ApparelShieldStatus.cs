using RimWorld;
using UnityEngine;
using Verse;

namespace RimElShrine.Apparels.ApparelShield
{
    [StaticConstructorOnStartup]
    public class Gizmo_ApparelShieldStatus(Color progressColor) : Gizmo_EnergyShieldStatus
    {
        private Color progressColor = progressColor;
        protected Texture2D? fullShieldBarTex = null;
        protected Texture2D FullShieldBarTex
        {
            get
            {
                var color = ((CompProperties_ApparelShield)shield.Props).CurrentIndexInfo.drawingColor;
                if (color != progressColor || fullShieldBarTex == null)
                {
                    progressColor = color;
                    fullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(color);
                }
                return fullShieldBarTex;
            }
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var shield = (Comp_ApparelShield)this.shield;
            Rect rect = new(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.height = rect.height / 2f;
            Text.Font = GameFont.Tiny;
            if (shield.Props.CurrentIndexInfo.overrideLabel != null) Widgets.Label(rect3, shield.Props.CurrentIndexInfo.overrideLabel);
            else if (shield.Props.CurrentIndexInfo.showApparelDescription && shield.IsApparel) Widgets.Label(rect3, shield.parent.LabelCap);
            else Widgets.Label(rect3, "ESShieldLabel".ES_Translate().Resolve());
            Rect rect4 = rect2;
            rect4.yMin = rect2.y + rect2.height / 2f;
            float fillPercent = shield.Energy / Mathf.Max(1f, shield.EnergyMax);
            Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            var progressBarLabel = shield.Energy > 0f ? 
                (shield.Energy * 100f).ToString("F0") + '/' + (shield.EnergyMax * 100f).ToString("F0") : 
                shield.TicksToReset.TicksToSeconds().ToString("F1");
            Widgets.Label(rect4, progressBarLabel);
            Text.Anchor = TextAnchor.UpperLeft;
            if (shield.Props.CurrentIndexInfo.overrideDescription != null) TooltipHandler.TipRegion(rect2, shield.Props.CurrentIndexInfo.overrideDescription);
            else if (shield.Props.CurrentIndexInfo.showApparelDescription) TooltipHandler.TipRegion(rect2, shield.parent.DescriptionDetailed);
            else TooltipHandler.TipRegion(rect2, "ESShieldTip".ES_Translate());
            return new GizmoResult(GizmoState.Clear);
        }

        // Token: 0x04005098 RID: 20632
        protected static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }
}
