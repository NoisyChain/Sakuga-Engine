using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CheckDistanceCondition : FrameDataCondition
    {
        [Export] private int MinDistance;
        [Export] private int MaxDistance;
        [Export] private RelativeTo RelativeTo;
        [Export] private int Index;
        [Export] private bool Not;
        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return false;

            int dist = GlobalFunctions.HorizontalDistance(Actor.Body.FixedPosition, Actor.GenerateTargetPosition(Vector2I.Zero, Index, RelativeTo, RelativeTo));

            if (Not) return dist < MinDistance || dist > MaxDistance;
            return dist >= MinDistance && dist <= MaxDistance;
        }
    }
}
