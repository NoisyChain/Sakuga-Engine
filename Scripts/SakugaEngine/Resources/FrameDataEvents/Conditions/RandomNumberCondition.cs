using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class RandomNumberCondition : FrameDataCondition
    {
        [Export] private Vector2I Range = new(0, int.MaxValue);
        [Export] private int Value;
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            int random = RNG.Next(Range.X, Range.Y);

            if (Not) return random != Value;
            return random == Value;
        }
    }
}
