using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ForcedKnockbackAction : FrameDataAction
    {
        [Export] private int KnockbackTime;
        [Export] private Vector2I KnockbackDirection;
        [Export] private int KnockbackGravity;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return;

            Actor.PushActor(
                KnockbackTime, 
                KnockbackDirection.X * Actor.Body.PlayerSide,
                KnockbackDirection.Y, 
                KnockbackGravity
            );
        }
    }
}
