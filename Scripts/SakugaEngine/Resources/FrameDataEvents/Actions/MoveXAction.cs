using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class MoveXAction : FrameDataAction
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

                if (Actor.Inputs.IsBeingPressed(Actor.Inputs.CurrentHistory, PlayerInputs.RIGHT))
                    InputSide = 1;
                else if (Actor.Inputs.IsBeingPressed(Actor.Inputs.CurrentHistory, PlayerInputs.LEFT))
                    InputSide = -1;

                if (UseInertia)
                    Actor.Body.AddLateralAcceleration(Velocity * InputSide);
                else
                    Actor.Body.SetLateralVelocity(Velocity * InputSide);
                
                return;
            }

            if (UseInertia)
                Actor.Body.AddLateralAcceleration(Velocity * Actor.InputSide);
            else
                Actor.Body.SetLateralVelocity(Velocity * Actor.InputSide);
        }
    }
}