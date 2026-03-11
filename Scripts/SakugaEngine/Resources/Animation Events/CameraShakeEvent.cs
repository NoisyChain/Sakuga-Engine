using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CameraShakeEvent : AnimationEvent
    {
        [Export] public float Amplitude;
        [Export] public float WaveLength;
        [Export] public int Duration;
        [Export] public bool SideRelative;
        [Export] public Vector2 Direction;
    }
}
