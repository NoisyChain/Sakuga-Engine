using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AIData : Resource
    {
        [ExportCategory("Actions")]
        [Export] public AIAction[] Actions;
        [Export] public int HighBlockAction;
        [Export] public int LowBlockAction;
        [Export] public int ForwardRecoveryAction;
        [Export] public int BackRecoveryAction;
        [Export] public int ThrowEscapeAction;

        [ExportCategory("Behaviors")]
        [Export] public AIBehavior BehaviorBeginner;
        [Export] public AIBehavior BehaviorEasy;
        [Export] public AIBehavior BehaviorMedium;
        [Export] public AIBehavior BehaviorHard;
        [Export] public AIBehavior BehaviorVeryHard;
        [Export] public AIBehavior BehaviorPro;
    }
}
