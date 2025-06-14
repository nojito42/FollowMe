using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using System.Drawing;

namespace FollowMe;

public class FollowMeSettings : ISettings
{
    //Mandatory setting to allow enabling/disabling your plugin
    public ToggleNode Enable { get; set; } = new ToggleNode(false);


    public ListNode PartyLeader { get; set; } = new ListNode();
    public ToggleNode UseMagicInput { get; set; } = new ToggleNode(false);
    //Put all your settings here if you can.
    //There's a bunch of ready-made setting nodes,
    //nested menu support and even custom callbacks are supported.
    //If you want to override DrawSettings instead, you better have a very good reason.
}