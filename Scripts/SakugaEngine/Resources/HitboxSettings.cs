using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
    public partial class HitboxSettings : Resource
    {
        [Export] public Vector2I PushboxCenter = Vector2I.Zero;
        [Export] public Vector2I PushboxSize = Vector2I.Zero;
        [Export] public HitboxElement[] Hitboxes;
    }
}
