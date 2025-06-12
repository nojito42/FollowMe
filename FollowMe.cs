using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Helpers;
using Microsoft.VisualBasic;
using SharpDX;
using System;
using System.Collections.Generic;
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

public class PartyLeader
{
    public string CharacterName { get; set; }
    public Element LeaderElement { get; set; }
    public Entity LeaderEntity { get; set; }
    public override string ToString()
    {
        return CharacterName;
    }

    public string WritePlayerInfos()
    {
        return $"{CharacterName} - {LeaderElement}";
    }
}
public class FollowMe : BaseSettingsPlugin<FollowMeSettings>
{

    public List<PartyElementPlayerElement> partyElements = [];
    public PartyElementPlayerInfo partyLeaderInfo = null;

    private DateTime _lastTeleportTime = DateTime.MinValue;
    private readonly TimeSpan _teleportCooldown = TimeSpan.FromMilliseconds(1000); // Cooldown de 5 secondes

    private void TryTeleportToLeader(PartyElementPlayerElement leaderElement)
    {
        if (DateTime.Now - _lastTeleportTime < _teleportCooldown)
        {
            //LogMessage("Teleport action on cooldown.");
            return;
        }

        var ui = this.GameController.IngameState.IngameUi;

        if (leaderElement.TeleportButton.IsActive && !GameController.Area.CurrentArea.IsHideout)
        {
            //LogMessage($"Teleporting to party leader {leaderElement.PlayerName} in {leaderElement.ZoneName}...");

            var centerTP = leaderElement.TeleportButton.GetClientRectCache.Center.ToVector2Num();
            Input.SetCursorPos(centerTP);
            Input.Click(MouseButtons.Left);

            if (ui.PopUpWindow != null && ui.PopUpWindow.ChildCount > 0)
            {
                Input.KeyPressRelease(Keys.Enter);
                LogMessage($"Teleported to party leader {leaderElement.PlayerName} in {leaderElement.ZoneName}.");
                var centerscreen = GameController.Window.GetWindowRectangle().Center.ToVector2Num();
                Input.SetCursorPos(centerscreen);
            }

            _lastTeleportTime = DateTime.Now;
        }
    }
    public override bool Initialise()
    {
        return true;
    }

    public override void AreaChange(AreaInstance area)
    {
    }

    public override Job Tick()
    {
        if (this.IsInParty() == false)
        {

        }
        if (this.IsInParty())
        {
            SetPartyListSettingsValues();

            var leaderElement = this.LeaderPlayerElement();
            var ui = this.GameController.IngameState.IngameUi;
            if (leaderElement != null)
            {
                if (partyLeaderInfo.IsInDifferentZone)
                {
                    TryTeleportToLeader(leaderElement);
                }
            }
        
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