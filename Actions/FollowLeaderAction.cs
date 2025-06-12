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
    private Vector2 previousLeaderPos = Vector2.Zero; // Stocke la position précédente du leader

    public int Priority => 1;
    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(45);
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

        // Vérifier la distance du leader
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

        // Vérifier si le leader bouge
        bool leaderIsMoving = previousLeaderPos != Vector2.Zero &&
                              previousLeaderPos != leaderEntity.GridPosNum;
        previousLeaderPos = leaderEntity.GridPosNum; // Mise à jour de la position précédente

        // Comparer la destination du joueur avec sa position actuelle
        bool playerAtTarget = playerPath != null &&
                              playerPath.PathingNodes.Count > 0 &&
                              playerPath.PathingNodes.Last() == playerEntity.GridPosNum;

        if (!leaderIsMoving && playerAtTarget)
        {
            // Générer une nouvelle position proche du leader pour éviter de cliquer au même endroit
            Vector2 newTarget = leaderEntity.GridPosNum + new Vector2(5, 5);

            // Vérifier si une compétence de déplacement est disponible
            var shortCut = plugin.AllSkills.FirstOrDefault(x => x.Skill.Name.Contains("Move"));
            var shortcuts = plugin.GameController.IngameState.ShortcutSettings.Shortcuts.Skip(7).Take(13).ToList();

            if (shortCut != null)
            {
                var sc = shortcuts[shortCut.Skill.SkillSlotIndex];
                if (sc.MainKey != ConsoleKey.None)
                {
                    var targetScreenPos = plugin.GameController.IngameState.Data.GetGridScreenPosition(newTarget);
                    if (targetScreenPos == Vector2.Zero) return;

                    Input.SetCursorPos(targetScreenPos);
                    Input.KeyPressRelease((Keys)sc.MainKey);
                    plugin.LogMessage($"Déplacement ajusté : {newTarget}, Skill : {shortCut.Skill.Name}");
                }
            }
        }
    }
}