using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Helpers;
using FollowMe.Actions;
using Microsoft.VisualBasic;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Vector2 = System.Numerics.Vector2;

namespace FollowMe;


public static class FollowMeHelpers
{
    public static string GetPartyLeaderName(this FollowMe plugin)
    {
        if (plugin.Settings.PartyLeader == null || plugin.Settings.PartyLeader.Value == null)
            return string.Empty;
        return plugin.Settings.PartyLeader.Value;
    }

    public static bool IsInParty(this FollowMe plugin)
    {
        return plugin.GameController.IngameState.IngameUi.PartyElement.PlayerElements.Count > 0;
    }

    public static PartyElementPlayerElement LeaderPlayerElement(this FollowMe plugin)
    {
        if (plugin.Settings.PartyLeader == null || plugin.Settings.PartyLeader.Value == null)
            return null;
        foreach (var playerElement in plugin.partyElements)
        {
            if (playerElement.PlayerName == plugin.Settings.PartyLeader.Value)
            {
                plugin.GameController.IngameState.IngameUi.PartyElement.Information.TryGetValue(playerElement.PlayerName, out var playerInfo);

                if (playerInfo != null)
                {
                    plugin.partyLeaderInfo = playerInfo;
                    plugin.LogMessage($"Found party leader: {playerInfo.IsInDifferentZone}");
                }


                return playerElement;
            }
        }
        return null;
    }

}
public class FollowMe : BaseSettingsPlugin<FollowMeSettings>
{
    private ActionManager actionManager;

    public List<PartyElementPlayerElement> partyElements = [];
    public PartyElementPlayerInfo partyLeaderInfo = null;
    public List<SkillElement> AllSkills;
    private List<GameOffsets.Shortcut> Shortcuts;

    public override bool Initialise()
    {
        actionManager = new ActionManager();
        actionManager.Register(new TeleportToLeaderAction(this));
        actionManager.Register(new FollowLeaderAction(this));
        return true;
    }

    public override void AreaChange(AreaInstance area)
    {
    }
    public override Job Tick()
    {
        if (this.IsInParty() )
        {
            SetPartyListSettingsValues();

            if(this.LeaderPlayerElement() != null && !MenuWindow.IsOpened)
                actionManager.Tick();


             AllSkills = this.GameController.IngameState.IngameUi.SkillBar.Skills;

             Shortcuts = [.. this.GameController.IngameState.ShortcutSettings.Shortcuts.Skip(5).Take(13)];


        }

        return null;
    }


    private void SetPartyListSettingsValues()
    {
        partyElements = this.GameController.IngameState.IngameUi.PartyElement.PlayerElements;

        Settings.PartyLeader.SetListValues([]);
        foreach (var partyElement in partyElements)
        {
            if (partyElement == null || partyElement.PlayerName == null)
                continue;
            Settings.PartyLeader.Values.Add(partyElement.PlayerName);
        }
    }

    public override void Render()
    {
        if (this.IsInParty() == false)
        {
            Graphics.DrawTextWithBackground("You are not in a party.", new Vector2(100, 100), Color.Red);
            return;
        }
        else
        {
            Graphics.DrawTextWithBackground($"You are in a party. Leader : {this.LeaderPlayerElement().PlayerName}({this.LeaderPlayerElement().ZoneName})...", new Vector2(100, 100), Color.Green);
        }
    }
}