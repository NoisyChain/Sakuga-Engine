using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AnimationDamageAction : FrameDataAction
    {
        [Export] private int Damage;
        [Export] private HitstunType HitstunType;
        [Export] private bool KillingBlow;
        [Export] private bool UseProrations;
        [Export] private bool AffectDamageTracker;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Parameters == null) return;

            Actor.HitstunType = HitstunType;

            int finalDamage = Damage;
            if (UseProrations && Actor.Parameters.Prorations != null)
            {
                finalDamage = Actor.Parameters.CalculateProrations(
                    Damage, 100,
                    Actor.Parameters.Prorations.CurrentDefense,
                    0
                );
            }
            Actor.Parameters.TakeDamage(
                finalDamage,
                0,
                KillingBlow
            );
            
            if (AffectDamageTracker && Actor.Parameters.Tracker != null)
                Actor.Parameters.Tracker.UpdateTrackers(finalDamage, 0, 1, 3, false);
        }
    }
}
