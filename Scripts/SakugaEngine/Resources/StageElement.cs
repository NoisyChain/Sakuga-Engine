using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class StageElement : Resource
    {
        [Export] public PackedScene Instance;
        [Export] public string Name;
        [Export] public Texture2D Thumbnail;
    }
}
