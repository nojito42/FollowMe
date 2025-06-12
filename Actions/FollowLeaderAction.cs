using ExileCore.Shared.Helpers;
using ExileCore;
using System;
using System.Numerics;
using System.Windows.Forms;
using System.Linq;
using ExileCore.PoEMemory.Components;

namespace FollowMe.Actions;

public class FollowLeaderAction(FollowMe plugin) : IGameAction
{
    private readonly FollowMe plugin = plugin;

    public int Priority => 1; // Priorité plus haute que TP par exemple
    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(300); // plus rapide que TP
    public string MutexKey => "followLeader";

    public bool CanExecute()
    {
        if (plugin.GameController.IsLoading)
            return false;

        if (plugin.partyLeaderInfo == null || plugin.partyLeaderInfo.IsInDifferentZone)
            return false;

        var leader = plugin.LeaderPlayerElement();
        if (leader == null)
            return false;

        var leaderEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Player]
            .FirstOrDefault(x => x.GetComponent<Player>().PlayerName == leader.PlayerName);

        if (leaderEntity == null)
            return false;

        if (plugin.GameController.Area.CurrentArea.IsHideout)
            return false;

        // Check distance sur leaderEntity
        if (leaderEntity.DistancePlayer <= 15)
            return false;

        return true;
    }


    public void Execute()
    {
        var leader = plugin.LeaderPlayerElement();
        if (leader == null) return;

        // Prendre la position à l’écran du leader
        var leaderEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Player]
           .FirstOrDefault(x => x.GetComponent<Player>().PlayerName == leader.PlayerName);
        var leaderScreenPos = plugin.GameController.IngameState.Camera.WorldToScreen(leaderEntity.PosNum);
        if (leaderScreenPos == Vector2.Zero) return;

        // Déplacer la souris vers la position du leader
        Input.SetCursorPos(leaderScreenPos);
        Input.Click(MouseButtons.Left);
        plugin.LogMessage($"Following leader {leader.PlayerName} at distance {leaderEntity.DistancePlayer}.");
    }
}
