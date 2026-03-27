using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class IsFallingCondition : FrameDataCondition
    {
        [Export] private bool Not;
        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return false;
            
            if (Not) return !Actor.Body.IsFalling;
            return Actor.Body.IsFalling;
        }
    }
}
