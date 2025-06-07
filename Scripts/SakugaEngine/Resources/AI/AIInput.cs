using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AIInput : Resource
    {
        [ExportSubgroup("Directional Inputs")]
        [Export] public Global.DirectionalInputs Direction;
        [ExportSubgroup("Face Button Inputs")]
        [Export] public Global.ButtonInputs Buttons;
    }
}
