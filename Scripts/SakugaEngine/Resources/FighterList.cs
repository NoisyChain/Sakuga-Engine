using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterList : Resource
    {
        [Export] public FighterElement[] elements;
    }
}

