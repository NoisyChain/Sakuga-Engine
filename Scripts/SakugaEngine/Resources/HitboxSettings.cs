using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
    [Icon("res://Sprites/Icons/Icon_Hitbox.png")]
    public partial class HitboxSettings : Resource
    {
        [Export] public BoxShape Pushbox;
        [Export] public HitboxElement[] Hitboxes;
    }
}
