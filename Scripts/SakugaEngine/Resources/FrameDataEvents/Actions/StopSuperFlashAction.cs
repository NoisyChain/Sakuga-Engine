using Godot;
using SakugaEngine.Game;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class StopSuperFlashAction : FrameDataAction
    {
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor == null) return;
            if (GameManager.Instance == null) return;

            Actor.StopSuperFlash();
        }
    }
}
