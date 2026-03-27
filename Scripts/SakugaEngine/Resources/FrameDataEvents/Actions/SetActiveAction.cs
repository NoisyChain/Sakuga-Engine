using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SetActiveAction : FrameDataAction
    {
        [Export] private bool Active;
        public override void Execute(ref SakugaActor Actor)
        {
            Actor.IsActive = Active;
        }
    }
}
