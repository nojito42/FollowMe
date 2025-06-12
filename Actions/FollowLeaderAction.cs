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
    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(80); // plus rapide que TP
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

        // Prendre la position à l’écran du leader
        var leaderEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Player]
           .FirstOrDefault(x => x.GetComponent<Player>().PlayerName == leader.PlayerName);
        var leaderScreenPos = plugin.GameController.IngameState.Camera.WorldToScreen(leaderEntity.PosNum);
        if (leaderScreenPos == Vector2.Zero) return;
        var ShortCut = plugin.AllSkills.FirstOrDefault(x => x.Skill.Name.Contains("Move"));   
        var shortcuts= plugin.GameController.IngameState.ShortcutSettings.Shortcuts.Skip(7).Take(13).ToList();
        if (ShortCut != null)
        {
           var sc = shortcuts[ShortCut.Skill.SkillSlotIndex];
           if(sc.MainKey != ConsoleKey.None)
            {
                var leaderPath = leaderEntity.GetComponent<Pathfinding>();
                if(leaderPath.PathingNodes.Count > 0 && leaderPath.IsMoving)
                {
                    leaderScreenPos = plugin.GameController.IngameState.Data.GetGridScreenPosition(leaderPath.PathingNodes.Last());
                    plugin.LogMessage($"Using pathfinding node at {leaderScreenPos} for leader {leader.PlayerName}.");
                }
                Input.SetCursorPos(leaderScreenPos);
                Input.KeyPressRelease((Keys)sc.MainKey); // Utiliser le raccourci principal
                plugin.LogMessage($"Skill: {ShortCut.Skill.InternalName} - {ShortCut.Skill.Name} - {sc.MainKey}");
            }
           
        }
        //    Input.SetCursorPos(leaderScreenPos);
        //    Input.Click(MouseButtons.Left);
        //    Input.KeyPressRelease(ShortCut.Skill.SkillSlotIndex + 1); // +1 car les raccourcis commencent à 1
        //    plugin.LogMessage($"Skill: {ShortCut.Skill.InternalName} - {ShortCut.Skill.Name} - {shortcuts[ShortCut.Skill.SkillSlotIndex]}");
        //}
        //else
        //{
        //    plugin.AllSkills.ForEach(x => plugin.LogMessage($"Skill: {x.Skill.InternalName} - {x.Skill.Name} - {shortcuts[x.Skill.SkillSlotIndex]}"));
        //    return;
        //}
        //// Déplacer la souris vers la position du leader
        //Input.SetCursorPos(leaderScreenPos);
        //Input.Click(MouseButtons.Left);
        //plugin.LogMessage($"Following leader {leader.PlayerName} at distance {leaderEntity.DistancePlayer}.");
    }
}
