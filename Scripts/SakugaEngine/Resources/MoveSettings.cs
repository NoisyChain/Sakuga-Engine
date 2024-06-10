using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class MoveSettings : Resource
    {
        [Export] public string MoveName;
        [Export] public int MoveState;
        [Export] public MotionInputs Command;
        [Export] public Global.MoveEndCondition MoveEnd;
        [Export] public int MoveEndState = -1;
        [Export] public int SuperGaugeRequired = 0;
        [Export] public int MinimumHealth = 0;
        [Export] public int MaximumHealth = 99999;
        [Export] public int SpendHealth = 0;
        [Export] public int ChangeStance = -1;
        [Export] public int[] IsSequenceFromStates;
        //[Export] public int[] CheckOpponentStates;
        [Export] public int[] CancelsTo;
        [Export] public int[] KaraCancelsTo;
        [Export] public ButtonChargeSequence[] buttonChargeSequence;
        [Export] public int SuperFlash = 0;
        [Export] public bool CanBeOverrided;
        [Export] public bool CanOverrideToSelf;
        [Export] public int Priority = 0;
        [Export] public bool UseOnGround, UseOnAir;
        //[Export] public bool AllowJumpCancel, AllowDashCancel;
    }
}
