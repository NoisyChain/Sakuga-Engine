using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class StageList : Resource
    {
        [Export] public StageElement[] elements;
    }
}
