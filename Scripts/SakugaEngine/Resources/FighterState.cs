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
        [Export] public Vector2I LoopFrames;
        [Export] public int TurnState = -1;
        [Export] public int HitStunFrameLimit = -1;
        [Export] public FrameProperties[] stateProperties;
        [Export] public StatePhysics[] statePhysics;
        [Export] public HitboxState[] hitboxStates;
        [Export] public AnimationEventsList[] animationEvents;
        [Export] public ThrowPivot[] throwPivot;
        [Export] public StateTransitionSettings[] stateTransitions;
        [Export] public AnimationSettings[] animationSettings;
        [ExportSubgroup("AI Flags")]
        [Export] public Global.AIFlags AIFlags;
    }
}