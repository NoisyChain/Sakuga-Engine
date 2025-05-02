using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AIInput : Resource
    {
        [Export] public Global.DirectionalInputs Direction = Global.DirectionalInputs.NEUTRAL;
        [Export] public Global.ButtonInputs Buttons = Global.ButtonInputs.NULL;
    }
}
