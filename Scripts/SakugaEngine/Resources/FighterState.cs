using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterState : Resource
    {
        [Export] public string StateName;
        [Export] public Global.StateType Type;
        [Export] public bool OffTheGround;
        [Export] public int Duration;
        [Export] public bool Loop;
        [Export] public int LoopStartFrame;
        [Export] public int LoopEndFrame;
        [Export] public int HitStunFrameLimit = -1;
        [Export] public StatePhysics[] statePhysics;
        /*
        [Export] public HitboxState[] hitboxStates;
        [Export] public AnimationEvents[] animationEvents;
        [Export] public ThrowPivot[] throwPivot;
        [Export] public SoundEvents[] soundEvents;
        */
        [Export] public StateTransitionSettings[] stateTransitions;
    }
}