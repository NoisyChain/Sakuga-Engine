using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class MatchAnimation : Resource
    {
        [Export] public string Name;
        [Export] public int Duration;
        [Export] public int DelaySubtract;
        [Export] public int AnnouncerLineIndex = -1;
    }
}
