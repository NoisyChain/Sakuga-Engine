using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CustomVariableBehavior : Resource
    {
        [Export] public Global.CustomVariableBehaviorTarget Target;
        [Export] public bool SetValue;
        [Export] public int Factor;
        [Export] public int Value;
        [Export] public bool IsRandom;
        [Export] public int Range;
        [Export] public Global.CustomVariableMode Mode;
    }
}
