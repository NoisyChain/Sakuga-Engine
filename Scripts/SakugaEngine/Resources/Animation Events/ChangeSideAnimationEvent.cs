using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ChangeSideAnimationEvent : AnimationEvent
    {
        [Export] public int Index;
    }
}
