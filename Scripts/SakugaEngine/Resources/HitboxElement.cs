using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
    public partial class HitboxElement : Resource
    {
        [Export] public Vector2I Center = Vector2I.Zero;
        [Export] public Vector2I Size = Vector2I.Zero;
        [Export] public Global.HitboxType HitboxType;
        [Export] public int SelectedAttackProfile = -1;
    }
}
