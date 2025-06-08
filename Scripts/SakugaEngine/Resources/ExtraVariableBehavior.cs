using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ExtraVariableBehavior : Resource
    {
        [Export] public bool SetValue;
        [Export] public int Factor;
        [Export] public int Value;
        [Export] public int RandomRange = -1;
        [Export] public Global.ExtraVariableMode Mode;
    }
}
