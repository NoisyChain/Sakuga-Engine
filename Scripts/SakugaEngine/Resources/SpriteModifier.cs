using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
    public partial class SpriteModifier : Resource
    {
        [Export] public Texture2D Sprite;
        [Export] public Vector2 Offset = new Vector2(-56, -4);
        [Export(PropertyHint.Range, "0, 360")] public int Rotation;
        [Export] public bool FlipX;
        [Export] public bool FlipY;
    }
}
