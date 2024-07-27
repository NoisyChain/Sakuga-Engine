using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class InputOption : Resource
    {
        [Export] public InputSequence[] Inputs;
    }
}
