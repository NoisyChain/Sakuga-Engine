using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class InputSequence : Resource
    {
        [ExportSubgroup("Directional Inputs")]
        [Export] public Global.DirectionalInputs Directional;
        [Export] public Global.ButtonMode DirectionalMode;
        [ExportSubgroup("Face Button Inputs")]
        [Export] public Global.ButtonInputs Buttons;
        [Export] public Global.ButtonMode ButtonMode;
    }
}
