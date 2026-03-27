using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CheckDistanceCondition : FrameDataCondition
    {
        [Export] private int MinDistance;
        [Export] private int MaxDistance;
        [Export] private bool Not;
        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return false;

            return false;
        }
    }
}
