using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimElShrine.Buildings.ProjInterceptorNet
{
    #region Terminal
    [StaticConstructorOnStartup]
    public class Comp_ShieldNet_Terminal : Comp_ShieldNet
    {
        public override void CompTickRare() => ShieldNet?.UpdateShieldNet();

        #region Sting and Gizmos
        private static Texture2D? resetAllTx2D;
        private static Texture2D ResetAllTx2D => resetAllTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldTerminal_ResetUpgradeResources");

        private static Texture2D? allocateTx2D;
        private static Texture2D AllocateTx2D => allocateTx2D ??= ContentFinder<Texture2D>.Get("Commands/ShieldTerminal_AllocateUpgradeResources");

        private static ShieldNetResAllocateWindow? allocateWindow;
        private static Window? AllocateWindow => allocateWindow;
       
        protected virtual void OpenAllocateWindow()
        {
            if ((allocateWindow is null || !allocateWindow.IsOpen)&& ShieldNet is not null)
            {
                allocateWindow = new ShieldNetResAllocateWindow(ShieldNet, parent);
            }
            if (allocateWindow is not null) Find.WindowStack.TryRemove(allocateWindow, false);
            Find.WindowStack.Add(allocateWindow);
        }
        private static readonly Action EmptyAction = () => { };
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var command_Action_ResetAll = new Command_Action()
            {
                defaultLabel = "ShieldNet_Command_ResetAllocatedResouces_Label".ES_Translate(),
                defaultDesc = "ShieldNet_Command_ResetAllocatedResouces_Desc1".ES_Translate(),
                icon = ResetAllTx2D,
                action = ShieldNet != null ? ShieldNet.ResetAllocatedResources : EmptyAction,
            };
            if (ShieldNet != null && ShieldNet.AvailableResources == ShieldNet.Resources) command_Action_ResetAll.Disable("ShieldNet_Command_ResetAllocatedResouces_DisabledReason1".ES_Translate());
            var command_Action_Allocate = new Command_Action()
            {
                defaultLabel = "ShieldNet_Command_AllocateResouces_Label".ES_Translate(),
                defaultDesc = "ShieldNet_Command_AllocateResouces_Desc".ES_Translate(),
                icon = AllocateTx2D,
                action = OpenAllocateWindow,
            };
            yield return DisableGizmoByShieldNet(command_Action_ResetAll).DisabledByCompFaction(parent.Faction); ;
            yield return DisableGizmoByShieldNet(command_Action_Allocate).DisabledByCompFaction(parent.Faction); ;
        }
        #endregion
    }
    #endregion
}
