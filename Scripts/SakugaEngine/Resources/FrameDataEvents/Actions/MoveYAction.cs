using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class MoveYAction : FrameDataAction
    {
        [Export] private int Velocity;
        [Export] public bool UseInertia;
        [Export] public bool UseInput;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return;
            if (Actor.OnKnockback()) return;

            if (UseInput && Actor.Inputs != null)
            {
                int InputSide = 0;

                if (Actor.Inputs.IsBeingPressed(Actor.Inputs.CurrentHistory, PlayerInputs.UP))
                    InputSide = 1;
                else if (Actor.Inputs.IsBeingPressed(Actor.Inputs.CurrentHistory, PlayerInputs.DOWN))
                    InputSide = -1;

                if (UseInertia)
                    Actor.Body.AddVerticalAcceleration(Velocity * InputSide);
                else
                    Actor.Body.SetVerticalVelocity(Velocity * InputSide);
                
                return;
            }

            if (UseInertia)
                Actor.Body.AddVerticalAcceleration(Velocity);
            else
                Actor.Body.SetVerticalVelocity(Velocity);
        }
    }
}