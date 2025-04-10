using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterElement : Resource
    {
        [Export] public PackedScene Instance;
        [Export] public FighterProfile Profile;
    }
}
