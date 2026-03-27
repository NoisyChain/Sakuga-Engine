using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CanBounceCondition : FrameDataCondition
    {
        [Export] private bool GroundBounce;
        [Export] private bool WallBounce;
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            bool canGroundBounce = GroundBounce && (Not ? Actor.IsBouncingY() : !Actor.IsBouncingY());
            bool canWallBounce = WallBounce && (Not ? Actor.IsBouncingX() : !Actor.IsBouncingX());

            return canGroundBounce || canWallBounce;
        }
    }
}
