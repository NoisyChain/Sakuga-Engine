using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class BGMList : Resource
    {
        [Export] public BGMElement[] elements;
    }
}
