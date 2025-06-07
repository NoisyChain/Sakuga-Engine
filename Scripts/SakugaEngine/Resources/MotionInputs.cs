using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class MotionInputs : Resource
    {
        [Export] public InputOption[] ValidInputs;
        [Export] public bool AbsoluteDirection;
        [Export] public int InputBuffer = 8;
        [Export] public int DirectionalChargeLimit = 0;
        [Export] public int ButtonChargeLimit = 0;
    }
}
