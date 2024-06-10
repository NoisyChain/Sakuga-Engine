using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
    public partial class HitboxSettings : Resource
    {
        [Export] private Vector2I PushboxCenter = Vector2I.Zero;
        [Export] private Vector2I PushboxSize = Vector2I.Zero;
        [Export] public HurtboxElement[] Hurtboxes;
        [Export] public HitboxElement[] Hitboxes;
    }
}
