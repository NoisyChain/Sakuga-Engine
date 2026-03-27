using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AtFrameCondition : FrameDataCondition
    {
        [Export] private int Frame;
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.StateManager == null) return false;

            if (Not) return Actor.StateManager.CurrentStateFrame != Frame;
            return Actor.StateManager.CurrentStateFrame == Frame;
        }
    }
}
