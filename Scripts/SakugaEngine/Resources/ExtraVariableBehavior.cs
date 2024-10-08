using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ExtraVariableBehavior : Resource
    {
        [Export] public int Factor;
        [Export] public Global.ExtraVariableMode Mode;
    }
}
