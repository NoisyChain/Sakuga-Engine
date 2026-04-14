using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CheckXVelocityCondition : FrameDataCondition
    {
        [Export] private bool Absolute;
        [Export] private int Value;
        [Export] private Global.CompareMode CompareMode;

        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return false;

            int vel = Absolute ? Mathf.Abs(Actor.Body.FixedVelocity.X) : Actor.Body.FixedVelocity.X;

            switch (CompareMode)
            {
                case Global.CompareMode.EQUAL:
                    return vel == Value;
                case Global.CompareMode.DIFFERENT:
                    return vel != Value;
                case Global.CompareMode.GREATER:
                    return vel > Value;
                case Global.CompareMode.GREATER_EQUAL:
                    return vel >= Value;
                case Global.CompareMode.LESS:
                    return vel < Value;
                case Global.CompareMode.LESS_EQUAL:
                    return vel <= Value;
            }

            return false;
        }
    }
}
