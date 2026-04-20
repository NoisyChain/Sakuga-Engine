using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    [Icon("res://Sprites/Icons/Icon_Motion.png")]
    public partial class MoveSettings : Resource
    {
        [ExportCategory("Move Parameters")]
        [Export] public string MoveName;
        [Export] public int MoveState;
        [Export] public MotionInputs Inputs;
        [Export] public int FrameLimit = -1;
        [Export] public int SuperFlash = 0;
        [Export] public int Priority = 0;
        [Export] public Global.MoveEndCondition MoveEnd;
        [Export] public int MoveEndState = -1;

        [ExportCategory("Move Conditions")]
        [Export] public int SuperGaugeRequired = 0;
        [Export] public Vector2I HealthRequired = new Vector2I(0, 99999);
        [Export] public Vector2I DistanceArea = new Vector2I(0, 999999);
        [Export] public int MinimumHeight = 0;
        [Export] public int[] IsSequenceFromStates;
        [Export] public int[] IgnoreStates;
        [Export] public MoveCancelSettings[] CanCancelTo;
        [Export] public CustomVariableCondition[] VariablesRequirement;
        [Export] public CustomVariableChange[] VariablesChange;
        
        [ExportCategory("Move Flags")]
        [Export] public bool SkipCheck;
        [Export] public bool CanBeOverrided;
        [Export] public bool CanOverrideToSelf;
        [Export] public bool PriorityBuffer;
        [Export] public bool IgnoreSamePriority = true;
        [Export] public bool AcceptNullStates = false;
        [Export] public bool AcceptMovementStates = true;
        [Export] public bool AcceptCombatStates = false;
        [Export] public bool AcceptBlockingStates = false;
        [Export] public bool AcceptHitReactionStates = false;
        [Export] public bool IgnoreHitstop = false;
        [Export] public bool IgnoreHitstun = false;
        [Export] public bool IgnoreKnockdownHits = false;
        [Export] public bool UseOnGround, UseOnAir;
    }
}
