using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ResetHitAction : FrameDataAction
    {
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return;

            Actor.Body.ResetHits();
        }
    }
}
