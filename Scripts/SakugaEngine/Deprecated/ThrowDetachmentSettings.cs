using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ThrowDetachmentSettings : Resource
    {
        [Export] public bool InvertSide;
        [Export] public Global.HitstunType HitstunType;
        [Export] public Vector2I HitKnockback;
        [Export] public int HitKnockbackGravity = 0;
        [Export] public int HitKnockbackTime = 8;
        [Export] public bool HitKnockbackInertia;
        [Export] public int Hitstun = 8;
        [Export] public int HorizontalBounce = 0;
        [Export] public int HorizontalBounceIntensity = 0;
        [Export] public int VerticalBounce = 0;
        [Export] public int VerticalBounceIntensity = 0;
    }
}