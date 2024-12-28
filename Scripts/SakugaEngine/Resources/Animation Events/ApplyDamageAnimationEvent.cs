using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ApplyDamageAnimationEvent : AnimationEvent
    {
        [Export] public int Index;
        [Export] public Global.ExtraVariableChange HealthChange;
        [Export] public int Value;
        [Export] public bool AffectedByModifiers;
        [Export] public bool AffectDamageTracker;
        [Export] public bool KillingBlow;
    }
}
