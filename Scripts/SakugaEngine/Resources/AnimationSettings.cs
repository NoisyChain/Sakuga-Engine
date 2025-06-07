using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AnimationSettings : Resource
    {
        [Export] public int AtFrame;
        [Export] public string SourceAnimation;
        [Export] public bool LimitRange;
        [Export] public Vector2I AnimationRange;
        [Export] public Vector3 Offset = Vector3.Zero;
    }
}
