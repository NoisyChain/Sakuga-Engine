using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CustomVariableCondition : Resource
    {
        [Export] public int ByIndex;
        [Export] public string ByName = "";
        [Export] public Global.CustomVariableMode Mode;
        [Export] public int Value;
        [Export] public Global.CompareMode CompareMode;
    }
}
