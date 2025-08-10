using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FrameDataSettings : Resource
    {
        [Export] public int Frame;
        [Export] public FrameProperties PropertySettings;
        [Export] public StatePhysics PhysicsSettings;
        [Export] public HitboxState HitboxSettings;
        [Export] public ThrowPivot ThrowAttachmentSettings;
        [Export] public AnimationEventsList AnimationEvents;
    }
}

