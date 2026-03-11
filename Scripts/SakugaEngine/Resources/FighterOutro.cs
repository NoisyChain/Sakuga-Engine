using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterOutro : Resource
    {
        [Export] public int StateIndex;
        [Export] public string ForOpponent = "";
        [Export(PropertyHint.MultilineText)] public string FinalMessage;
    }
}
