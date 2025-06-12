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
    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(45); // plus rapide que TP
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
        if (leaderEntity.DistancePlayer <= 22)
            return false;

        return true;
    }


    public void Execute()
    {
        var leader = plugin.LeaderPlayerElement();
        if (leader == null) return;

        var leaderEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Player]
            .FirstOrDefault(x => x.GetComponent<Player>().PlayerName == leader.PlayerName);

        var playerEntity = plugin.GameController.Game.IngameState.Data.LocalPlayer;
        var playerPath = playerEntity.GetComponent<Pathfinding>();

        var leaderPath = leaderEntity?.GetComponent<Pathfinding>();
        if (leaderPath == null || leaderPath.PathingNodes.Count == 0) return;

        // Comparer les destinations si le joueur est déjà en mouvement
        if (playerPath != null && playerPath.IsMoving && playerPath.PathingNodes.Count > 0)
        {
            var playerTarget = playerPath.PathingNodes.Last();
            var leaderTarget = leaderPath.PathingNodes.Last();

            float distance = Vector2.Distance(playerTarget, leaderTarget); 
            plugin.LogMessage($"Distance to leader's target: {distance}");
            //if (distance < 10f) // tolérance de 10 unités
            //{
            //    plugin.LogMessage("Déjà en route vers une destination proche de celle du leader.");
            //    return;
            //}

        }

        // Préparer l'utilisation du skill de déplacement
        var shortCut = plugin.AllSkills.FirstOrDefault(x => x.Skill.Name.Contains("Move"));
        var shortcuts = plugin.GameController.IngameState.ShortcutSettings.Shortcuts.Skip(7).Take(13).ToList();

        if (shortCut != null)
        {
            var sc = shortcuts[shortCut.Skill.SkillSlotIndex];
            if (sc.MainKey != ConsoleKey.None)
            {
                var leaderScreenPos = plugin.GameController.IngameState.Data.GetGridScreenPosition(leaderPath.PathingNodes.Last());

                if (leaderScreenPos == Vector2.Zero)
                    leaderScreenPos = plugin.GameController.IngameState.Camera.WorldToScreen(leaderEntity.PosNum);

                if (leaderScreenPos == Vector2.Zero) return;

                Input.SetCursorPos(leaderScreenPos);
                Input.KeyPressRelease((Keys)sc.MainKey);
                plugin.LogMessage($"Skill: {shortCut.Skill.InternalName} - {shortCut.Skill.Name} - {sc.MainKey}");
            }
        }
    }

}
