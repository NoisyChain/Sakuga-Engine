using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class StateEndedCondition : FrameDataCondition
    {
        [Export] private bool Not;
        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.StateManager == null) return false;

            if (Not) return Actor.StateManager.CurrentStateFrame < Actor.StateManager.GetCurrentState().AnimationData.Duration;
            return Actor.StateManager.CurrentStateFrame >= Actor.StateManager.GetCurrentState().AnimationData.Duration;
        }
    }
}
