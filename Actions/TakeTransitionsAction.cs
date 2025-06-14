using ExileCore.Shared.Helpers;
using ExileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.PoEMemory.Components;

namespace FollowMe.Actions;

public class TakeTransitionsAction(FollowMe plugin) : IGameAction
{
    private readonly FollowMe plugin = plugin;
    private Entity cachedTransitionEntity;

    public int Priority => 0;
    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(25);
    public string MutexKey => "teleport";

    public bool CanExecute()
    {
        var leader = plugin.LeaderPlayerElement();
        if (leader == null || plugin.GameController?.EntityListWrapper == null)
            return false;

        var leaderEntity = plugin.GameController.EntityListWrapper
            .ValidEntitiesByType.GetValueOrDefault(EntityType.Player)?
            .FirstOrDefault(e => e?.GetComponent<Player>()?.PlayerName == leader.PlayerName);

        if (leaderEntity == null)
            return false;



        // Transition candidates
        var transitions = plugin.GameController.EntityListWrapper
            .ValidEntitiesByType[EntityType.AreaTransition | EntityType.Portal | EntityType.TownPortal]
            .Where(e => e?.RenderName == leader.ZoneName).
            OrderBy(e => e.DistancePlayer)
            .ToList();
        plugin.LogMessage($"[Follow] Found {transitions.Count} transitions for '{leader.ZoneName}' at distance {leaderEntity.DistancePlayer:F1}.", 1, SharpDX.Color.GreenYellow);

        cachedTransitionEntity = transitions.FirstOrDefault();
        if (cachedTransitionEntity == null)
            return false;

        var leaderActionTarget = leaderEntity.GetComponent<Actor>()?.CurrentAction?.Target;
        if (leaderActionTarget == null)
            return false;

        return leaderActionTarget == cachedTransitionEntity &&
               !plugin.GameController.IsLoading &&
                
               !plugin.GameController.Area.CurrentArea.IsHideout;
    }

    public void Execute()
    {
        if (cachedTransitionEntity == null)
            return;

        if (cachedTransitionEntity.DistancePlayer > 55)
        {
            plugin.LogMessage($"[Follow] Transition '{cachedTransitionEntity.RenderName}' trop loin ({cachedTransitionEntity.DistancePlayer:F1}).", 1, SharpDX.Color.Yellow);
            return;
        }

        if(plugin.Settings.UseMagicInput)
        {
            plugin.GameController.PluginBridge
                .GetMethod<Action<Entity, uint>>("MagicInput.TeleportToEntity")
                .Invoke(cachedTransitionEntity, 0x400);
        }
        else
        {
            var screenPos = plugin.GameController.IngameState.Data.GetGridScreenPosition(cachedTransitionEntity.GridPosNum);
            if (screenPos == Vector2.Zero)
            {
                plugin.LogError("[Follow] Position écran invalide pour la transition.");
                return;
            }

            plugin.LogMessage($"[Follow] Téléportation vers '{cachedTransitionEntity.RenderName}' à l’écran {screenPos}.", 1, SharpDX.Color.Green);
            Input.SetCursorPos(screenPos);
            Input.Click(MouseButtons.Left);
        }
       
    }
}
