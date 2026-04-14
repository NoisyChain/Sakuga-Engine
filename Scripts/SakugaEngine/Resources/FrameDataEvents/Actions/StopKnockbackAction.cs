using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class StopKnockbackAction : FrameDataAction
    {
        [Export] private bool KeepXVelocity;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return;

            Actor.StopKnockback(KeepXVelocity);
        }
    }
}
