using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AtFrameRangeCondition : FrameDataCondition
    {
        [Export] private int MinFrame;
        [Export] private int MaxFrame;
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.StateManager == null) return false;

            if (Not) return Actor.StateManager.CurrentStateFrame < MinFrame || Actor.StateManager.CurrentStateFrame > MaxFrame;
            return Actor.StateManager.CurrentStateFrame >= MinFrame && Actor.StateManager.CurrentStateFrame <= MaxFrame;
        }
    }
}
