using Godot;
using SakugaEngine.Game;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class StartSuperFlashAction : FrameDataAction
    {
        [Export] private ushort Duration;
        [Export] private bool SetImmune;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor == null) return;
            if (GameManager.Instance == null) return;

            Actor.StartSuperFlash(Duration, SetImmune);
        }
    }
}
