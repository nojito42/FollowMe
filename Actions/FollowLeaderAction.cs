using ExileCore.Shared.Helpers;
using ExileCore;
using System;
using System.Numerics;
using System.Windows.Forms;
using System.Linq;
using ExileCore.PoEMemory.Components;
using GameOffsets.Native;
using ExileCore.Shared.Enums;

namespace FollowMe.Actions;

public class FollowLeaderAction(FollowMe plugin) : IGameAction
{
    private readonly FollowMe plugin = plugin;

    public int Priority => 2; // Priorité plus haute que TP par exemple
    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(45); // plus rapide que TP
    public string MutexKey => "followLeader";

    public bool CanExecute()
    {
        if (plugin.GameController.IsLoading)
            return false;

        var currentArea = plugin.GameController.Area.CurrentArea;
        if(currentArea.IsTown && !plugin.Settings.FollowInTown)
            return false;
        var leader = plugin.LeaderPlayerElement();
        if (leader == null)
            return false;
        var transitions = plugin.GameController.EntityListWrapper
           .ValidEntitiesByType
           .Where(kvp =>
               kvp.Key.HasFlag(EntityType.AreaTransition) ||
               kvp.Key.HasFlag(EntityType.Portal) ||
               kvp.Key.HasFlag(EntityType.TownPortal))
           .SelectMany(kvp => kvp.Value)
           .Where(e => e?.RenderName == leader.ZoneName).
           OrderBy(e => e.DistancePlayer)
           .ToList();
        var playerAction = plugin.GameController.Player.GetComponent<Actor>()?.CurrentAction;
        if(playerAction != null && transitions.Any(p=> p == playerAction.Target))
            return false;

        if (plugin.partyLeaderInfo == null || plugin.partyLeaderInfo.IsInDifferentZone)
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

    var leaderEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Player]
        .FirstOrDefault(x => x.GetComponent<Player>().PlayerName == leader.PlayerName);
    if (leaderEntity == null) return;

    var leaderPath = leaderEntity.GetComponent<Pathfinding>();
    if (leaderPath == null) return;

    var playerEntity = plugin.GameController.Game.IngameState.Data.LocalPlayer;
    var playerPath = playerEntity.GetComponent<Pathfinding>();

    Vector2 targetPos;
    if (leaderPath.IsMoving && leaderPath.PathingNodes.Count > 0)
        targetPos = leaderPath.PathingNodes.Last();
    else
        targetPos = leaderEntity.GridPosNum;

    if (playerPath != null && playerPath.IsMoving && playerPath.PathingNodes.Count > 0)
    {
        var playerTarget = playerPath.PathingNodes.Last();
        float distance = Vector2.Distance(playerTarget, targetPos);

        if (distance < 10f)
        {
            return;
        }
    }

    try
    {
            if (plugin.Settings.UseMagicInput)
            {

                var castWithPos = plugin.GameController.PluginBridge
                    .GetMethod<Action<Vector2i, uint>>("MagicInput.CastSkillWithPosition");
                castWithPos(targetPos.TruncateToVector2I(), 0x400);
            }
            else
            {

                var wts = plugin.GameController.IngameState.Data.GetGridScreenPosition(targetPos);
                Input.SetCursorPos(wts);
                var moveSkill = plugin.GameController.IngameState.IngameUi.SkillBar.Skills
                    .FirstOrDefault(x => x.Skill.IsOnSkillBar && x.Skill.Id == 10505); // ID du skill de déplacement
                if (moveSkill != null)
                {
                    var sc = plugin.shortcuts.Skip(7).Take(13).ToList()[moveSkill.Skill.SkillSlotIndex];
                    Input.KeyPressRelease((Keys)sc.MainKey);
                }
            }
        }
    catch (Exception ex)
    {
        plugin.LogError($"[Follow] Échec du cast via PluginBridge : {ex.Message}");
    }
}
}
