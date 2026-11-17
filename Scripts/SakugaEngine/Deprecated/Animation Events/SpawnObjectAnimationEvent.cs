using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SpawnObjectAnimationEvent : AnimationEvent
    {
        [Export] public Global.ObjectType Object;
        [Export] public Vector2I TargetPosition = Vector2I.Zero;
        [Export] public Global.RelativeTo xRelativeTo, yRelativeTo;
        [Export] public int Index;
        [Export] public bool IsRandom;
        [Export] public int Range;
        [Export] public int FromExtraVariable = -1;
        [Export] public bool FollowParent;
    }
}