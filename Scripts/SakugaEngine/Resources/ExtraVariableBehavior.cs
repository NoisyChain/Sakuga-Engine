using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ExtraVariableBehavior : Resource
    {
        [Export] public bool SetValue;
        [Export] public int Factor;
        [Export] public int Value;
        [Export] public bool IsRandom;
        [Export] public int Range;
        [Export] public Global.ExtraVariableMode Mode;
    }
}
