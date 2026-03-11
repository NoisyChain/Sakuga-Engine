using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ColorPaletteElement : Resource
    {
        [Export] public string PaletteName;
        [Export] public PackedScene Instance;
    }
}

