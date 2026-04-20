using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CanBounceWallCondition : FrameDataCondition
    {
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (!Actor.CanBounce()) return false;
            if (Not) return !Actor.IsBouncingX();

            return Actor.IsBouncingX();
        }
    }
}
