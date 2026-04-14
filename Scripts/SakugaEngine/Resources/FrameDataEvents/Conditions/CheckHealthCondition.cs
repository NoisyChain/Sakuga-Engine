using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CheckHealthCondition : FrameDataCondition
    {
        [Export] private int Value;
        [Export] private Global.CompareMode CompareMode;
        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.Parameters == null) return false;
            if (Actor.Parameters.Health == null) return false;
            
            switch (CompareMode)
            {
                case Global.CompareMode.EQUAL:
                    return Actor.Parameters.Health.CurrentValue == Value;
                case Global.CompareMode.DIFFERENT:
                    return Actor.Parameters.Health.CurrentValue != Value;
                case Global.CompareMode.GREATER:
                    return Actor.Parameters.Health.CurrentValue > Value;
                case Global.CompareMode.GREATER_EQUAL:
                    return Actor.Parameters.Health.CurrentValue >= Value;
                case Global.CompareMode.LESS:
                    return Actor.Parameters.Health.CurrentValue < Value;
                case Global.CompareMode.LESS_EQUAL:
                    return Actor.Parameters.Health.CurrentValue <= Value;
            }

            return false;
        }
    }
}
