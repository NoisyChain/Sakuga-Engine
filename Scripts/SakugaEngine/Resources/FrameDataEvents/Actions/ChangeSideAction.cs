using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ChangeSideAction : FrameDataAction
    {
        [Export] private bool Forced;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return;

            if (Forced) Actor.Body.UpdateSide(!Actor.Body.IsLeftSide);

            Actor.Body.FlipSide();
        }
    }
}
