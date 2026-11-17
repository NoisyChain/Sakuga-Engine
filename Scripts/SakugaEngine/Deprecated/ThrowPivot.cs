using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ThrowPivot : Resource
    {
        [Export] public int Frame;
        [Export] public Vector2I PivotPosition = Vector2I.Zero;
        [Export] public int ThrowState = -1;
        [Export] public ThrowDetachmentSettings ThrowDetach;
    }
}
//TODO: Considering to add interpolation support for this thing