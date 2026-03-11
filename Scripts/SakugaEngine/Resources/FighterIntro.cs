using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterIntro : Resource
    {
        [Export] public int StateIndex;
        [Export] public string ForOpponent = "";
    }
}
