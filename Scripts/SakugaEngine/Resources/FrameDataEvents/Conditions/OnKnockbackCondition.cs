using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class OnKnockbackCondition : FrameDataCondition
    {
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (!Actor.CanKnockback()) return false;

            if (Not) return !Actor.OnKnockback();
            return Actor.OnKnockback();
        }
    }
}
