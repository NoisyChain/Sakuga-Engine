using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ExtraVariableChange : Resource
    {
        [Export] public Global.ExtraVariableChange ChangeMode;
        [Export] public uint ChangeValue;
    }
}