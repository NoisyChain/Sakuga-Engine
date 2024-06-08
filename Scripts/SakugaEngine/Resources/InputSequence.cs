using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class InputSequence : Resource
    {
        [Export] public Global.DirectionalInputs Directional = Global.DirectionalInputs.NEUTRAL;
        [Export] public Global.ButtonInputs Buttons = Global.ButtonInputs.NULL;
        [Export] public Global.ButtonMode DirectionalMode;
        [Export] public Global.ButtonMode ButtonMode;
    }
}
