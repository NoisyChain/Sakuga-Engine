using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AddSuperGaugeAction : FrameDataAction
    {
        [Export] private int Value;
        [Export] private bool Absolute;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Value == 0) return;
            if (Actor.Parameters == null) return;
            if (Actor.Parameters.SuperGauge == null) return;
            
            if (Absolute)
            {
                Actor.Parameters.SuperGauge.SetSuperGauge(Value);
                return;
            }
            
            if (Value > 0)
                Actor.Parameters.SuperGauge.AddSuperGauge(Value);
            else if (Value < 0)
                Actor.Parameters.SuperGauge.SpendSuperGauge(Value);
        }
    }
}
