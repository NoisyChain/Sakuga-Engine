using Godot;
using SakugaEngine.Game;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ThrowPivotAction : FrameDataAction
    {
        [Export] private Vector2I PivotPosition = Vector2I.Zero;
        [Export] private int ThrowReaction = -1;
        [Export] private bool EndThrow;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.CurrentGrabbedNodeID < 0) return;

            SakugaActor grabbedActor = GameManager.Instance.GetActor(Actor.CurrentGrabbedNodeID) as SakugaActor;
             GD.Print(grabbedActor.Name);
            if (grabbedActor == null) return;
            if (grabbedActor.Body == null) return;
            if (grabbedActor.HitstunType != HitstunType.GRABBED) return;

            int grabState = ThrowReaction >= 0 ? grabbedActor.StanceManager.GetCurrentStance().HitReactions[ThrowReaction] : -1;
            grabbedActor.StateManager.PlayState(grabState, false);
            grabbedActor.Body.SetVelocity(Vector2I.Zero);
            grabbedActor.Body.MoveTo(Actor.Body.FixedPosition + PivotPosition * new Vector2I(Actor.Body.PlayerSide, 1));

            if (EndThrow) Actor.CurrentGrabbedNodeID = -1;
        }
    }
}
