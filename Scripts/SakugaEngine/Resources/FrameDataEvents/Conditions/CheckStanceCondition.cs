using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CheckStanceCondition : FrameDataCondition
    {
        [Export] private int CurrentStance;
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.StanceManager == null) return false;

            if (Not) return Actor.StanceManager.CurrentStance != CurrentStance;
            return Actor.StanceManager.CurrentStance == CurrentStance;
        }
    }
}
