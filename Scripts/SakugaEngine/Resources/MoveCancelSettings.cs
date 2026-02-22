using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class MoveCancelSettings : Resource
    {
        [Export] public int MoveIndex;
        [Export] public Global.CancelCondition Conditions;
        [Export] public Vector2I FrameThreshold;
    }
}