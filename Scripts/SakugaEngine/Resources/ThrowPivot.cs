using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ThrowPivot : Resource
    {
        [Export] public int Frame;
        [Export] public Vector2I PivotPosition = Vector2I.Zero;
        [Export] public int ThrowState = -1;
        [Export] public bool Dettach;
        [Export] public bool DettachInvertSide;
        [Export] public Global.HitstunType DettachHitstunType;
        [Export] public Vector2I DettachHitKnockback;
        [Export] public int DettachHitKnockbackGravity = 0;
        [Export] public int DettachHitKnockbackTime = 8;
        [Export] public bool DettachHitKnockbackInertia;
        [Export] public int DettachHitstun = 8;
    }
}
//TODO: Considering to add interpolation support for this thing