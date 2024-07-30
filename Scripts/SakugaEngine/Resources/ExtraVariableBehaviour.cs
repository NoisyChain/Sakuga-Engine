using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ExtraVariableBehaviour : Resource
    {
        [Export] public uint Factor;
        [Export] public Global.ExtraVariableMode Mode;
    }
}
