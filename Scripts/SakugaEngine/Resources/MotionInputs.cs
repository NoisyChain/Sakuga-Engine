using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class MotionInputs : Resource
    {
        [Export] public InputSequence[] Inputs;
        [Export] public bool AbsoluteDirection;
        [Export] public int InputBuffer = 8;
        [Export] public int DirectionalChargeLimit = 30;
        [Export] public int ButtonChargeLimit = 30;
    }
}
