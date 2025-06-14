using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Helpers;
using FollowMe.Actions;
using GameOffsets.Native;
using Microsoft.VisualBasic;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Vector2 = System.Numerics.Vector2;
using Shortcut = GameOffsets.Shortcut;
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
                    //plugin.LogMessage($"Found party leader: {playerInfo.IsInDifferentZone}");
                }


                return playerElement;
            }
        }
        return null;
    }

}
public class FollowMe : BaseSettingsPlugin<FollowMeSettings>
{
    public TakeTransitionsAction TakeTransitionAction { get; private set; }
    public IList<GameOffsets.Shortcut> shortcuts { get; private set; }

    private ActionManager actionManager;

    public List<PartyElementPlayerElement> partyElements = [];
    public PartyElementPlayerInfo partyLeaderInfo = null;
    public List<SkillElement> AllSkills;
    private List<GameOffsets.Shortcut> Shortcuts;
    int tries = 0;

    public override bool Initialise()
    {


        var mem = GameController.Memory;
        var address = GameController.IngameState.ShortcutSettings.Address;
        //int maxTries = 10000;
        //IList<Shortcut> sc = new List<Shortcut>();
        //StdVector vec = new StdVector();
        //while ((sc.Count <= 10 || sc.Count > 1000) && tries < maxTries)
        //{
        //    vec = mem.Read<StdVector>(address + (500 + tries));
        //    sc = mem.ReadStdVector<Shortcut>(vec);
        //    tries++;
        //}

        var vec2 = mem.Read<StdVector>(address + (785-1));
        IList<Shortcut> sc3 = mem.ReadStdVector<Shortcut>(vec2);
        shortcuts = sc3;

        LogMessage(shortcuts.Count + " WTF" + "");
        LogMessage(shortcuts.Count + " WTF" + "");



        //var mem = GameController.Memory;
        //var address = GameController.IngameState.ShortcutSettings.Address;
        //int maxTries = 10000;
        //int tries = 0;
        //IList<GameOffsets.Shortcut> sc2 = new List<GameOffsets.Shortcut>();
        //StdVector vec = new StdVector();
        //while((sc2.Count <= 0 || sc2.Count > 1000) && tries < maxTries)
        //{
        //    vec = mem.Read<StdVector>(address - tries);
        //    sc2 = mem.ReadStdVector<GameOffsets.Shortcut>(vec);
        //    tries++;
        //}
        //var vec2 = mem.Read<StdVector>(address + tries-1);
        //IList<GameOffsets.Shortcut> sc3 = mem.ReadStdVector<GameOffsets.Shortcut>(vec2);
        //shortcuts = sc3;

        LogMessage($"Found {shortcuts.Count} shortcuts after {tries} tries.", 1, SharpDX.Color.GreenYellow);
        TakeTransitionAction = new TakeTransitionsAction(this);
        actionManager = new ActionManager();
        actionManager.Register(new TeleportToLeaderAction(this));
        actionManager.Register(new FollowLeaderAction(this));
       // actionManager.Register(new UseAttackSkillAction(this));
       actionManager.Register(TakeTransitionAction); 
        return true;
    }

    public override void AreaChange(AreaInstance area)
    {
    }
    public override Job Tick()
    {

       LogMessage("shortcuts count : " + shortcuts.Count + " " + tries, 1, SharpDX.Color.GreenYellow);

        shortcuts.ToList().ForEach(shortcut =>
        {
            LogMessage($"Shortcut: {shortcut}", 1, SharpDX.Color.GreenYellow);
        });
        if (this.IsInParty() )
        {
            SetPartyListSettingsValues();

            var skillBar = this.GameController.IngameState.IngameUi.SkillBar;

            var skills = skillBar.Skills
                .Where(x => x.Skill.IsOnSkillBar)
                .ToList();
            var portalTransitions = this.GameController.EntityListWrapper.ValidEntitiesByType[ExileCore.Shared.Enums.EntityType.AreaTransition]
                .Where(x => x.IsValid)
                .ToList();

            //portalTransitions.ForEach(transition =>
            //{
               
            //        this.LogMessage($"Found portal transition with same zone name: {transition.RenderName} at {transition.DistancePlayer}.",1,SharpDX.Color.GreenYellow);
                
            //});
            foreach (var skill in skills)
            {

                this.LogMessage($"Found Move skill on skill bar at index {skill.Skill} {skill.Skill.Id} {skill.Skill.Id2}.");

            }

            if (this.LeaderPlayerElement() != null)
            {
                if(partyLeaderInfo != null)
                {
                    if(!partyLeaderInfo.IsInDifferentZone)
                    {
                        var leader = GameController.Entities
                            .FirstOrDefault(x => x.GetComponent<Player>()?.PlayerName == this.LeaderPlayerElement().PlayerName);
                        if(leader != null)
                        {
                            var skillList =leader.GetComponent<ExileCore.PoEMemory.Components.Actor>()?.ActorSkills
                              .Where(x => x.IsOnSkillBar)
                              .ToList();

                            skillList.ForEach(skill =>
                            {
                                
                                   // LogMessage($"Skill: {skill.Name} is on skill bar. -> {skill.SkillSlotIndex} -");
                                
                            });

                        }
                    }
                }
            }
           
            if (this.LeaderPlayerElement() != null && !MenuWindow.IsOpened)
            {
                actionManager.Tick();

            }


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
        if (Settings.Enable == false ||MenuWindow.IsOpened)
            return;
        if (this.IsInParty() == false || GameController.IsLoading)
        {
            Graphics.DrawTextWithBackground("You are not in a party.", new Vector2(100, 100), Color.Red);
            return;
        }
        else
        {
            //Graphics.DrawTextWithBackground($"You are in a party. Leader : {this.LeaderPlayerElement().PlayerName}({this.LeaderPlayerElement().ZoneName})...", new Vector2(100, 100), Color.Green);
        }
    }
}