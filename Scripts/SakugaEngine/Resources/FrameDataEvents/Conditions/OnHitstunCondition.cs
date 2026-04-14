using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class OnHitstunCondition : FrameDataCondition
    {
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (!Actor.CanHitstun()) return false;

            if (Not) return !Actor.OnHitstun();
            return Actor.OnHitstun();
        }
    }
}
