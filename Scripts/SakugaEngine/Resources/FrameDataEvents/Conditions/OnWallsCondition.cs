using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class OnWallsCondition : FrameDataCondition
    {
        [Export] private bool Not;
        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return false;
            
            if (Not) return !Actor.Body.IsOnWall;
            return Actor.Body.IsOnWall;
        }
    }
}
