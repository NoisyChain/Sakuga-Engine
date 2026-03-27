using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CheckMoveCondition : FrameDataCondition
    {
        [Export] private int CheckSpecificMove;
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.StanceManager == null) return false;

            if (CheckSpecificMove <= 0)
            {
                if (Not) return Actor.StanceManager.CurrentMove < 0;
                return Actor.StanceManager.CurrentMove >= 0;
            }

            if (Not) return Actor.StanceManager.CurrentMove != CheckSpecificMove;
            return Actor.StanceManager.CurrentMove == CheckSpecificMove;
        }
    }
}
