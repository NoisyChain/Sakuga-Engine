using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CustomVariableChange : Resource
    {
        [Export] public int ByIndex;
        [Export] public string ByName = "";
        [Export] public Global.CustomVariableMode Mode;
        [Export] public int Value;
        [Export] public bool IsRandom;
        [Export] public int Range = -1;
    }
}