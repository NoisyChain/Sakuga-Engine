using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
    public partial class HurtboxElement : Resource
    {
        [Export] private Vector2I Center = Vector2I.Zero;
        [Export] private Vector2I Size = Vector2I.Zero;
    }
}
