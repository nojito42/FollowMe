using ExileCore.Shared.Helpers;
using ExileCore;
using System;
using System.Numerics;
using System.Windows.Forms;
using System.Linq;
using ExileCore.PoEMemory.Components;
using GameOffsets.Native;

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
            plugin.LogMessage("[Follow] Déjà en route vers la cible, skip mouvement.");
            return;
        }
    }

    try
    {
        var castWithPos = plugin.GameController.PluginBridge
            .GetMethod<Action<Vector2i, uint>>("MagicInput.CastSkillWithPosition");

        castWithPos(targetPos.TruncateToVector2I(), 0x400);
        plugin.LogMessage($"[Follow] Déplacement (CastSkillWithPosition) vers {targetPos}");
    }
    catch (Exception ex)
    {
        plugin.LogError($"[Follow] Échec du cast via PluginBridge : {ex.Message}");
    }
}


    //public void Execute()
    //{
    //    var leader = plugin.LeaderPlayerElement();
    //    if (leader == null) return;

    //    var leaderEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Player]
    //        .FirstOrDefault(x => x.GetComponent<Player>().PlayerName == leader.PlayerName);
    //    if (leaderEntity == null) return;

    //    var leaderPath = leaderEntity.GetComponent<Pathfinding>();
    //    if (leaderPath == null) return;

    //    var playerEntity = plugin.GameController.Game.IngameState.Data.LocalPlayer;
    //    var playerPath = playerEntity.GetComponent<Pathfinding>();

    //    Vector2 targetPos;

    //    // Si le leader est en mouvement, on prend le dernier point de son path
    //    if (leaderPath.IsMoving && leaderPath.PathingNodes.Count > 0)
    //    {
    //        targetPos = leaderPath.PathingNodes.Last();
    //    }
    //    else
    //    {
    //        // Sinon, on prend simplement sa position actuelle
    //        targetPos = leaderEntity.GridPosNum;
    //    }

    //    // Check si on est déjà en train de marcher vers une position proche
    //    if (playerPath != null && playerPath.IsMoving && playerPath.PathingNodes.Count > 0)
    //    {
    //        var playerTarget = playerPath.PathingNodes.Last();
    //        float distance = Vector2.Distance(playerTarget, targetPos);

    //        plugin.LogMessage($"[Follow] Distance to leader target: {distance}");

    //        if (distance < 10f) // Tolérance
    //        {
    //            plugin.LogMessage("[Follow] Déjà en route vers la cible, skip mouvement.");
    //            return;
    //        }
    //    }

    //    // Utilisation du skill Move
    //    var shortCut = plugin.AllSkills.FirstOrDefault(x => x.Skill.Name.Contains("Move"));
    //    var shortcuts = plugin.GameController.IngameState.ShortcutSettings.Shortcuts.Skip(7).Take(13).ToList();

    //    if (shortCut != null)
    //    {
    //        var sc = shortcuts[shortCut.Skill.SkillSlotIndex];
    //        if (sc.MainKey != ConsoleKey.None)
    //        {
    //            // Conversion de la cible en position écran
    //            var screenPos = plugin.GameController.IngameState.Data.GetGridScreenPosition(targetPos);
    //            if (screenPos == Vector2.Zero) return;

    //            Input.SetCursorPos(screenPos);
    //            Input.KeyPressRelease((Keys)sc.MainKey);
    //            plugin.LogMessage($"[Follow] Déplacement vers leader: {targetPos} via {sc.MainKey}");
    //        }
    //    }
    //}


}
