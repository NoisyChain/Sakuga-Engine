using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SuperArmorAnimationEvent : AnimationEvent
    {
        [Export] public int ArmorValue;
    }
}
