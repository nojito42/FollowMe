using ExileCore.Shared.Helpers;
using ExileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FollowMe.Actions;

public class TeleportToLeaderAction : IFollowerAction
{
    private readonly FollowMe plugin;
    private DateTime lastExecution = DateTime.MinValue;
    private int attempts = 0;

    public TeleportToLeaderAction(FollowMe plugin)
    {
        this.plugin = plugin;
    }

    public string Name => "TeleportToLeader";
    public int Priority => 10;
    public TimeSpan MinInterval => TimeSpan.FromMilliseconds(800);
    public TimeSpan Timeout => TimeSpan.FromSeconds(5);
    public int MaxAttempts => 3;

    public bool CanExecute()
    {
        var now = DateTime.Now;
        if (now - lastExecution < MinInterval || attempts >= MaxAttempts) return false;

        var leader = plugin.LeaderPlayerElement();
        return leader != null &&
               plugin.partyLeaderInfo != null &&
               plugin.partyLeaderInfo.IsInDifferentZone &&
               leader.TeleportButton.IsActive &&
               !plugin.GameController.Area.CurrentArea.IsHideout;
    }

    public void Execute()
    {
        attempts++;
        lastExecution = DateTime.Now;

        var leader = plugin.LeaderPlayerElement();
        if (leader == null) return;

        var tpCenter = leader.TeleportButton.GetClientRectCache.Center.ToVector2Num();
        Input.SetCursorPos(tpCenter);
        Input.Click(MouseButtons.Left);

        var ui = plugin.GameController.IngameState.IngameUi;
        if (ui.PopUpWindow?.ChildCount > 0)
        {
            Input.KeyPressRelease(Keys.Enter);
            Input.SetCursorPos(plugin.GameController.Window.GetWindowRectangle().Center.ToVector2Num());
        }

        plugin.LogMessage($"[Action] Teleported to {leader.PlayerName}.");
    }
}
