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

        var ui = plugin.GameController.IngameState.IngameUi;
        tpPos = leader.TeleportButton.GetClientRect().Center.ToVector2Num();

        Input.SetCursorPos(tpPos);
        Input.Click(MouseButtons.Left);

        if (ui.PopUpWindow != null && ui.PopUpWindow.ChildCount > 0)
        {
            Input.KeyPressRelease(Keys.Enter);

        }
    }
}


