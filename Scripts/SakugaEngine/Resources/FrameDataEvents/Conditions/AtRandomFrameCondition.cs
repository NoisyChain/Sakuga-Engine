using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AtRandomFrameCondition : FrameDataCondition
    {
        [Export] private Vector2I Range;
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.StateManager == null) return false;

            int random = RNG.Next(Range.X, Range.Y);

            if (Not) return Actor.StateManager.CurrentStateFrame != random;
            return Actor.StateManager.CurrentStateFrame == random;
        }
    }
}
