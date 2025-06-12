using ExileCore.Shared.Helpers;
using ExileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FollowMe.Actions;

public class TeleportToLeaderAction : IGameAction
{
    private readonly FollowMe plugin;

    public TeleportToLeaderAction(FollowMe plugin)
    {
        this.plugin = plugin;
    }

    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(1000);

    public bool CanExecute()
    {
        var leader = plugin.LeaderPlayerElement();
        return leader != null &&
               plugin.partyLeaderInfo != null &&
               plugin.partyLeaderInfo.IsInDifferentZone &&
               leader.TeleportButton?.IsActive == true &&
               !plugin.GameController.Area.CurrentArea.IsHideout;
    }

    public void Execute()
    {
        var leader = plugin.LeaderPlayerElement();
        var ui = plugin.GameController.IngameState.IngameUi;

        if (leader == null) return;

        var tpPos = leader.TeleportButton.GetClientRectCache.Center.ToVector2Num();
        Input.SetCursorPos(tpPos);
        Input.Click(MouseButtons.Left);

        if (ui.PopUpWindow != null && ui.PopUpWindow.ChildCount > 0)
        {
            Input.KeyPressRelease(Keys.Enter);
            plugin.LogMessage($"Teleported to {leader.PlayerName} in {leader.ZoneName}.");
            Input.SetCursorPos(plugin.GameController.Window.GetWindowRectangle().Center.ToVector2Num());
        }
    }
}

