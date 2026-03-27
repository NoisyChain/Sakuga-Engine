using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SetHealthAction : FrameDataAction
    {
        [Export] private int Value;
        [Export] private bool Absolute;
        [Export] private bool KillingBlow;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Value == 0) return;
            if (Actor.Parameters == null) return;
            if (Actor.Parameters.Health == null) return;
            
            if (Absolute)
            {
                Actor.Parameters.Health.SetHealth(Value);
                return;
            }
            
            if (Value > 0)
                Actor.Parameters.Health.AddHealth(Mathf.Abs(Value));
            else if (Value < 0)
            {
                Actor.Parameters.Health.RemoveHealth(Mathf.Abs(Value));
                if (KillingBlow && Actor.Parameters.Health.CurrentValue <= 0)
                    Actor.Parameters.Health.SetHealth(1);
            }
        }
    }
}
