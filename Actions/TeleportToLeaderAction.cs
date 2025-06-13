using ExileCore.Shared.Helpers;
using ExileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using ExileCore.Shared.Enums;

namespace FollowMe.Actions;

public class TeleportToLeaderAction(FollowMe plugin) : IGameAction
{
    private readonly FollowMe plugin = plugin;

    public int Priority => 1;
    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(500);
    public string MutexKey => "teleport";

    public bool CanExecute()
    {
        var leader = plugin.LeaderPlayerElement();

       
        return leader != null &&
            plugin.GameController.IsLoading == false &&
            plugin.TakeTransitionAction.CanExecute() == false&&
               plugin.partyLeaderInfo != null &&
               plugin.partyLeaderInfo.IsInDifferentZone &&
               leader.TeleportButton?.IsActive == true &&
               !plugin.GameController.Area.CurrentArea.IsHideout;
    }

    public void Execute()
    {
        var leader = plugin.LeaderPlayerElement();
        if (leader == null) return;

        var tpPos = Vector2.Zero;

        var potentialLabelWithSameZoneName = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.AreaTransition]
                .FirstOrDefault(x => x.RenderName == leader.ZoneName);
        var ui = plugin.GameController.IngameState.IngameUi;


        if (potentialLabelWithSameZoneName != null) plugin.LogMessage($"Found potential label with same zone name: {potentialLabelWithSameZoneName.RenderName} at {potentialLabelWithSameZoneName.DistancePlayer}.");
        if (potentialLabelWithSameZoneName != null && potentialLabelWithSameZoneName.DistancePlayer <= 55)
        {

            var wts = plugin.GameController.IngameState.Camera.WorldToScreen(potentialLabelWithSameZoneName.BoundsCenterPosNum);
            if (wts != System.Numerics.Vector2.Zero)
            {
                tpPos = wts;
                Input.SetCursorPos(tpPos);
                Input.Click(MouseButtons.Left);
                if (ui.PopUpWindow != null && ui.PopUpWindow.ChildCount > 0)
                {
                    Input.KeyPressRelease(Keys.Enter);

                }
                return;

            }
        }

        tpPos = leader.TeleportButton.GetClientRect().Center.ToVector2Num();

        Input.SetCursorPos(tpPos);
        Input.Click(MouseButtons.Left);

        if (ui.PopUpWindow != null && ui.PopUpWindow.ChildCount > 0)
        {
            Input.KeyPressRelease(Keys.Enter);

        }
    }
}


