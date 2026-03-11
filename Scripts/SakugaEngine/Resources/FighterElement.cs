using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterElement : Resource
    {
        [Export] public FighterProfile Profile;
        [Export] public ColorPaletteElement[] ColorPalettes;
    }
}
