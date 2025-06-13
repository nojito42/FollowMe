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

namespace FollowMe.Actions;

public class TakeTransitionsAction(FollowMe plugin) : IGameAction
{
    private readonly FollowMe plugin = plugin;
    private Entity potentialLabelWithSameZoneName;

    public int Priority => 0;
    public TimeSpan Cooldown => TimeSpan.FromMilliseconds(25);
    public string MutexKey => "teleport";

    public bool CanExecute()
    {
        var leader = plugin.LeaderPlayerElement();

         potentialLabelWithSameZoneName = plugin.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.AreaTransition | EntityType.Portal | EntityType.TownPortal]
            .FirstOrDefault(x => x.RenderName == leader.ZoneName);


        return leader != null &&
            plugin.GameController.IsLoading == false &&
               plugin.partyLeaderInfo != null &&
               plugin.partyLeaderInfo.IsInDifferentZone &&
               !plugin.GameController.Area.CurrentArea.IsHideout;
    }

    public void Execute()
    {
        var leader = plugin.LeaderPlayerElement();
        if (leader == null) return;

        var tpPos = Vector2.Zero;

    


        if (potentialLabelWithSameZoneName != null) plugin.LogMessage($"Found potential label with same zone name: {potentialLabelWithSameZoneName.RenderName} at {potentialLabelWithSameZoneName.DistancePlayer}.");
        if (potentialLabelWithSameZoneName != null && potentialLabelWithSameZoneName.DistancePlayer <= 55)
        {

            plugin.LogMessage($"Teleporting to {potentialLabelWithSameZoneName.RenderName}.");
            var wts = plugin.GameController.IngameState.Camera.WorldToScreen(potentialLabelWithSameZoneName.BoundsCenterPosNum);
            if (wts != System.Numerics.Vector2.Zero)
            {
                plugin.LogMessage($"Teleporting to {potentialLabelWithSameZoneName.RenderName} at {wts}.");
                tpPos = wts;
                Input.SetCursorPos(tpPos);
                Input.Click(MouseButtons.Left);
            
                return;

            }
        }

    }
}


