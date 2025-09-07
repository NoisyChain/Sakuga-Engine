using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AIAction : Resource
    {
        [Export] private string InputName; // For identification purposes
        [Export] public AIInput[] Inputs;
        [Export] public bool AutoAdvance;
    }
}