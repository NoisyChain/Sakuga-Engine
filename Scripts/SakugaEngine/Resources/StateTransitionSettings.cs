using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class StateTransitionSettings : Resource
    {
        [Export] public int StateIndex = -1;
        [Export] public Global.TransitionCondition Condition;
        [Export] public int AtFrame = -1;
        [Export] public MotionInputs Inputs;
    }
}
