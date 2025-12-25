using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AnimationSettings : Resource
    {
        [Export] public int AtFrame;
        [Export] public string SourceAnimation;
        [Export] public float Speed = 1f;
        [Export] public int FrameStepping = 1;
        [Export] public bool LimitRange;
        [Export] public Vector2I AnimationRange;
        [Export] public Vector3 Offset = Vector3.Zero;
    }
}
