using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ForcedKnockbackAction : FrameDataAction
    {
        [Export] private HitstunType HitstunType;
        [Export] private int KnockbackHitReaction;
        [Export] private int KnockbackTime;
        [Export] private Vector2I KnockbackDirection;
        [Export] private int KnockbackGravity;
        [Export] private int Hitstop = 0;
        [Export] private int Hitstun = 8;
        [Export] private int BounceTime = 0;
        [Export] private int HorizontalBounceIntensity = 0;
        [Export] private int VerticalBounceIntensity = 0;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return;

            Actor.HitstunType = HitstunType;

            if (Actor.CanHitstop() && Hitstop > 0) Actor.Hitstop.Start((uint)Hitstop);
            if (Actor.CanHitstun() && Hitstun > 0) Actor.Hitstun.Start((uint)Hitstun);

            Actor.StateManager.PlayState(Actor.StanceManager.GetCurrentStance().HitReactions[KnockbackHitReaction]);

            Actor.PushActor(
                KnockbackTime, 
                KnockbackDirection.X * Actor.Body.PlayerSide,
                KnockbackDirection.Y, 
                KnockbackGravity
            );

            Actor.StartBounce((uint)BounceTime, HorizontalBounceIntensity, VerticalBounceIntensity);
        }
    }
}
