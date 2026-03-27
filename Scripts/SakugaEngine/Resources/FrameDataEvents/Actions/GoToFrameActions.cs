using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class GoToFrameActions : FrameDataAction
    {
        [Export] private int Frame;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.StateManager == null) return;
            
            Actor.StateManager.CurrentStateFrame = Frame;
        }
    }
}
