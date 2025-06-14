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
using GameOffsets.Native;
using System.Threading;

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
        try { 
        var leader = plugin.LeaderPlayerElement();
        if (leader == null || plugin.GameController?.EntityListWrapper == null)
            return false;

        var leaderEntity = plugin.GameController.EntityListWrapper
            .ValidEntitiesByType.GetValueOrDefault(EntityType.Player)?
            .FirstOrDefault(e => e?.GetComponent<Player>()?.PlayerName == leader.PlayerName);

        if (leaderEntity == null)
            return false;

        var leaderActionTarget = leaderEntity.GetComponent<Actor>()?.CurrentAction?.Target;
        if (leaderActionTarget == null)
        {
            plugin.LogMessage($"[Follow] No action target for leader '{leader.PlayerName}' at transition '{cachedTransitionEntity.RenderName}'.", 1, SharpDX.Color.Yellow);
            return false;
        }
        else
        {
            plugin.LogMessage($"[Follow] Leader '{leader.PlayerName}' action target is '{leaderActionTarget.RenderName}' at distance {leaderEntity.DistancePlayer:F1}.", 1, SharpDX.Color.GreenYellow);
                cachedTransitionEntity = leaderActionTarget;
                return true;
        }


        // Transition candidates
        var transitions = plugin.GameController.EntityListWrapper
            .OnlyValidEntities
            .Where(e => e.DistancePlayer < 25 && (e.Type == EntityType.Portal || e.Type == EntityType.TownPortal || e.Type == EntityType.AreaTransition)).
            OrderBy(e => e.DistancePlayer)
            .ToList();
        plugin.LogMessage($"[Follow] Found {transitions.Count} transitions for '{leader.ZoneName}' at distance {leaderEntity.DistancePlayer:F1}.", 1, SharpDX.Color.GreenYellow);

        cachedTransitionEntity = transitions.FirstOrDefault();
        if (cachedTransitionEntity == null)
            return false;


        var myActionTarget = plugin.GameController.Player.GetComponent<Actor>()?.CurrentAction?.Target;

        if (myActionTarget != null && myActionTarget == cachedTransitionEntity)
        {
            plugin.LogMessage($"[Follow] Already at transition '{cachedTransitionEntity.RenderName}'.", 1, SharpDX.Color.Green);
            return false;
        }
        

        return  leaderActionTarget == cachedTransitionEntity;
        }
        catch (Exception ex)
        {
            plugin.LogError($"[Follow] Error in CanExecute: {ex.Message}");
            return false;
        }
    }

    public void Execute()
    {
        if (cachedTransitionEntity == null)
            return;

        var wts = plugin.GameController.IngameState.Camera.WorldToScreen(cachedTransitionEntity.BoundsCenterPosNum);
        if (wts == Vector2.Zero)
        {
            plugin.LogError("[Follow] Position écran invalide pour la transition.");
            return;
        }

        if (plugin.Settings.UseMagicInput)
        {

            plugin.GameController.PluginBridge
                    .GetMethod<Action<Vector2i, uint>>("MagicInput.CastSkillWithPosition")
                .Invoke(wts.RoundToVector2I(), 0x400);
        }
        else
        {






            //plugin.LogMessage($"[Follow] Téléportation vers '{cachedTransitionEntity.RenderName}' à l’écran {pl.Label.PositionNum}.", 1, SharpDX.Color.Green);
            // Set cursor position to the center of the label

            Input.SetCursorPos(wts);
        Thread.Sleep(10);
        Input.KeyDown(Keys.LButton);
            Input.KeyUp(Keys.LButton);
        }

    }
}
