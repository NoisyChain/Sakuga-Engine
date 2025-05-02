using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AIAction : Resource
    {
        [Export] public AIInput[] Inputs;
        [Export] public bool AutoAdvance;
    }
}