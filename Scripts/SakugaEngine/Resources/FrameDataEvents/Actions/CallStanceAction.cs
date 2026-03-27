using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CallStanceAction : FrameDataAction
    {
        [Export] private int NextStance;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.StanceManager == null) return;

            Actor.StanceManager.CallStance(NextStance);
        }
    }
}
