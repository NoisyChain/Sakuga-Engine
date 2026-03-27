using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class TeleportAnimationEvent : AnimationEvent
    {
        [Export] public int Index;
        [Export] public Vector2I TargetPosition = Vector2I.Zero;
        [Export] public Global.RelativeTo xRelativeTo, yRelativeTo;
    }
}
