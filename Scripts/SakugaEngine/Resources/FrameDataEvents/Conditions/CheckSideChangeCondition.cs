using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CheckSideChangeCondition : FrameDataCondition
    {
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return false;

            if (Not) return (Actor.Body.IsLeftSide && Actor.InputSide > 0) || (!Actor.Body.IsLeftSide && Actor.InputSide < 0);
            return (Actor.Body.IsLeftSide && Actor.InputSide < 0) || (!Actor.Body.IsLeftSide && Actor.InputSide > 0);
        }
    }
}
