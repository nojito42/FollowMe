using ExileCore.Shared.Helpers;
using ExileCore;
using System;
using System.Numerics;
using System.Windows.Forms;
using System.Linq;
using ExileCore.PoEMemory.Components;
using GameOffsets.Native;

namespace FollowMe.Actions;

public class UseAttackSkillAction(FollowMe plugin) : IGameAction
{
    private readonly FollowMe plugin = plugin;

    public int Priority => 2;
    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(45);
    public string MutexKey => "followLeader";

    public bool CanExecute()
    {
        if (plugin.GameController.IsLoading)
            return false;

        if (plugin.partyLeaderInfo == null || plugin.partyLeaderInfo.IsInDifferentZone)
            return false;

        if (plugin.GameController.Area.CurrentArea.IsHideout)
            return false;

        var leader = plugin.LeaderPlayerElement();
        if (leader == null)
            return false;

        var leaderEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Player]
            .FirstOrDefault(x => x.GetComponent<Player>().PlayerName == leader.PlayerName);

        if (leaderEntity == null || leaderEntity.DistancePlayer <= 15)
            return false;

        var monsterEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Monster]
            .Where(x => x.DistancePlayer < 50 && x.IsAlive)
            .OrderBy(x => x.DistancePlayer);

        return monsterEntity.Any();
    }

    public void Execute()
    {
        var monsterEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Monster]
            .Where(x => x.IsAlive)
            .OrderBy(x => x.DistancePlayer)
            .FirstOrDefault();

        if (monsterEntity == null)
        {
            plugin.LogMessage("[Follow] Aucun monstre valide trouvé.");
            return;
        }

        Vector2 targetPos = monsterEntity.GridPosNum;

        try
        {
            var skills = plugin.GameController.IngameState.IngameUi.SkillBar.Skills.FirstOrDefault(x => (x.Skill.IsAttack || x.Skill.IsSpell) && x.Skill.IsOnCooldown == false && x.Skill.CanBeUsed);
             ushort skillId = skills.Skill.Id; // À remplacer par ton ID plus tard

            var castWithPos = plugin.GameController.PluginBridge
                .GetMethod<Action<Vector2i, uint>>("MagicInput.CastSkillWithPosition");

            castWithPos(targetPos.TruncateToVector2I(), skillId);
            plugin.LogMessage($"[Follow] Attaque vers {targetPos} avec skillId {skillId:X}");
        }
        catch (Exception ex)
        {
            plugin.LogError($"[Follow] Erreur d'attaque : {ex.Message}");
        }
    }
}
