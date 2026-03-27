using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class TeleportAction : FrameDataAction
    {
        [Export] private int Index;
        [Export] private Vector2I TargetPosition = Vector2I.Zero;
        [Export] private Global.RelativeTo xRelativeTo, yRelativeTo;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return;
            
            Actor.Body.MoveTo(Actor.GenerateTargetPosition(TargetPosition, Index, xRelativeTo, yRelativeTo));
        }
    }
}
