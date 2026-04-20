using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AtFrameCondition : FrameDataCondition
    {
        [Export] private int Frame;
        [Export] private int Modulo;
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.StateManager == null) return false;

            if (Modulo > 0)
            {
                if (Not) return Actor.StateManager.CurrentStateFrame % Modulo != Frame;
                return Actor.StateManager.CurrentStateFrame % Modulo == Frame;
            }

            if (Not) return Actor.StateManager.CurrentStateFrame != Frame;
            return Actor.StateManager.CurrentStateFrame == Frame;
        }
    }
}
