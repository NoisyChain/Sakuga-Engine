using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AICondition : Resource
    {
        [Export] public Vector2I Distance;
        [Export] public bool UseOnGround, UseOnAir;
        [Export] public int SuperGaugeRequired;
        [Export] public int Probability = 10;
        [Export] public Global.BotMode ActionMode;
        [Export] public int[] ActionsList;
        [ExportSubgroup("AI Flags")]
        [Export] public Global.AIFlags CounterFlags;
    }
}
