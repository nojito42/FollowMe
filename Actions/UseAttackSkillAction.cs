using ExileCore.Shared.Helpers;
using ExileCore;
using System;
using System.Numerics;
using System.Windows.Forms;
using System.Linq;
using ExileCore.PoEMemory.Components;
using GameOffsets.Native;
using ExileCore.PoEMemory.MemoryObjects;

namespace FollowMe.Actions;
public class UseAttackSkillAction : IGameAction
{
    private readonly FollowMe plugin;
    private DateTime lastCast = DateTime.MinValue;
    private uint currentSkillId = 0;  // départ à 0x400

    public UseAttackSkillAction(FollowMe plugin)
    {
        this.plugin = plugin;
    }

    public int Priority => 3;
    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(45);
    public string MutexKey => "followLeader";

    public bool CanExecute()
    {
        // Toujours true pour test
        return true;
    }

    public void Execute()
    {
        if ((DateTime.Now - lastCast).TotalMilliseconds < 1)
            return;

        lastCast = DateTime.Now;

        try
        {
            var pos = plugin.GameController.Player.GridPosNum;
            Vector2i position = new Vector2i((int)pos.X, (int)pos.Y);

            plugin.GameController.PluginBridge
                .GetMethod<Action<Entity, uint>>("MagicInput.CastSkillWithTarget")
                .Invoke(plugin.GameController.EntityListWrapper.OnlyValidEntities[new Random().Next(plugin.GameController.EntityListWrapper.OnlyValidEntities.Count-1)], 32771-0x400);

            plugin.LogMessage($"[Follow] CastSkillWithPosition appelé en {position} avec skillId {currentSkillId:X}");

            currentSkillId++;

            if (currentSkillId > 0x10000)
                currentSkillId = 0;
        }
        catch (Exception ex)
        {
            plugin.LogError($"[Follow] Erreur d'attaque : {ex.Message}");
        }
    }
}


//public class UseAttackSkillAction(FollowMe plugin) : IGameAction
//{
//    private readonly FollowMe plugin = plugin;

//    public int Priority => 2;
//    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(45);
//    public string MutexKey => "followLeader";

//    public bool CanExecute()
//    {
//        if (plugin.GameController.IsLoading)
//            return false;

//        if (plugin.partyLeaderInfo == null || plugin.partyLeaderInfo.IsInDifferentZone)
//            return false;

//        if (plugin.GameController.Area.CurrentArea.IsHideout)
//            return false;

//        var leader = plugin.LeaderPlayerElement();
//        if (leader == null)
//            return false;
//        var monsterEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Monster]
//            .Where(x => x.DistancePlayer < 50 && x.IsAlive)
//            .OrderBy(x => x.DistancePlayer);

//        return monsterEntity.Any();
//    }

//    public void Execute()
//    {
//        var monsterEntity = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.Monster]
//            .Where(x => x.IsAlive)
//            .OrderBy(x => x.DistancePlayer)
//            .FirstOrDefault();

//        if (monsterEntity == null)
//        {
//            plugin.LogMessage("[Follow] Aucun monstre valide trouvé.");
//            return;
//        }

//        Vector2 targetPos = monsterEntity.GridPosNum;

//        try
//        {
//            var skills = plugin.GameController.IngameState.IngameUi.SkillBar.Skills.FirstOrDefault(x => (x.Skill.IsAttack || x.Skill.IsSpell) && (x.Skill.IsOnCooldown == false && x.Skill.CanBeUsed));
//             uint skillId = skills.Skill.Id; 

//            plugin.GameController.PluginBridge
//                .GetMethod<Action<Entity, uint>>("MagicInput.CastSkillWithTarget").Invoke(monsterEntity,0x400);


//            plugin.LogMessage($"[Follow] Attaque vers {targetPos} avec skillId {skillId:X}");
//        }
//        catch (Exception ex)
//        {
//            plugin.LogError($"[Follow] Erreur d'attaque : {ex.Message}");
//        }
//    }
//}
