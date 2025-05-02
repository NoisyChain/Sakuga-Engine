using Godot;
using System;


namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AIActionPack : Resource
    {
        [Export] public int HorizontalDistance;
        [Export] public AICondition[] Conditions;
    }
}
