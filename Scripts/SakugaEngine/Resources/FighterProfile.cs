using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterProfile : Resource
    {
        [Export] public string FighterName;
        [Export] public string ShortName;
        [Export] public Texture2D Render;
        [Export] public Texture2D Portrait;
        [Export] public int AutoStage = 0;
        [Export] public int AutoBGM = 0;
    }
}
