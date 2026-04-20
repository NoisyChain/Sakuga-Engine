using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
    public partial class BoxShape : Resource
    {
        [Export] public Vector2I Center;
        [Export] public Vector2I Size;
    }
}
