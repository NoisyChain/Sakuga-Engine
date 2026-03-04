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
        [Export] public bool SkipCheck;
        [Export] public MotionInputs Inputs;
        [Export] public Global.SideChangeMode SideChange;
        [Export] public int ChangeStance = -1;
        [Export] public int FrameLimit = -1;
        [Export] public int SuperFlash = 0;
        [Export] public int Priority = 0;
        [ExportCategory("Move Conditions")]
        [Export] public Global.MoveEndCondition MoveEnd;
        [Export] public int MoveEndState = -1;
        [Export] public int SuperGaugeRequired = 0;
        [Export] public int BuildSuperGauge = 0;
        [Export] public Vector2I HealthThreshold = new Vector2I(0, 99999);
        [Export] public int SpendHealth = 0;
        [Export] public Vector2I DistanceArea = new Vector2I(0, 999999);
        [Export] public int MinimumHeight = 0;
        [Export] public int[] IsSequenceFromStates;
        [Export] public int[] IgnoreStates;
        [Export] public MoveCancelSettings[] CanCancelTo;
        [Export] public ExtraVariableCondition[] ExtraVariablesRequirement;
        [Export] public ExtraVariableChange[] ExtraVariablesChange;
        
        [ExportCategory("Move Flags")]
        [Export] public bool CanBeOverrided;
        [Export] public bool CanOverrideToSelf;
        [Export] public bool PriorityBuffer;
        [Export] public bool IgnoreSamePriority = true;
        [Export] public bool InterruptCornerPushback = false;
        [Export] public bool AcceptNullStates = false;
        [Export] public bool AcceptCombatStates = false;
        [Export] public bool AcceptBlockingStates = false;
        [Export] public bool AcceptHitReactionStates = false;
        [Export] public bool IgnoreHitstop = false;
        [Export] public bool IgnoreHitstun = false;
        [Export] public bool UseOnGround, UseOnAir;
    }
}
