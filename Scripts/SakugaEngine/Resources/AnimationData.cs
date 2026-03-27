using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
    public partial class AnimationData : Resource
    {
        [Export] public int Duration;
        [Export] public int HitstunHold = -1;
        [Export] public AnimationSettings[] Animations;
        [Export] public HitboxState[] Hitboxes;
    }
}
