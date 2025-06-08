using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class PlaySoundAnimationEvent : AnimationEvent
    {
        [Export] public Global.SoundType SoundType;
        [Export] public int Source;
        [Export] public int Index;
        [Export] public bool IsRandom;
        [Export] public int Range;
        [Export] public int FromExtraVariable = -1;
    }
}
