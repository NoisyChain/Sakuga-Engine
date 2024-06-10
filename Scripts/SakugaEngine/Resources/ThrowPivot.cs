using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ThrowPivot : Resource
    {
        [Export] public int Frame;
        [Export] private Vector2I PivotPosition = Vector2I.Zero;
    }
}
//TODO: Considering to add interpolation support for this thing