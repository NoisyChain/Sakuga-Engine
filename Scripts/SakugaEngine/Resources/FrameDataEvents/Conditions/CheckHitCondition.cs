using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CheckHitCondition : FrameDataCondition
    {
        [Export] private Global.CancelCondition HitCheck;
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (Not) return (Actor.CancelConditions & HitCheck) == 0;
            return (Actor.CancelConditions & HitCheck) != 0;
        }
    }
}
