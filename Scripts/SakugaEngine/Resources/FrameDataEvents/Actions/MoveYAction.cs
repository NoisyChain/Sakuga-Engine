using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class MoveYAction : FrameDataAction
    {
        [Export] private string FromConstants;
        [Export] private int Velocity;
        [Export] public bool UseInertia;
        [Export] public bool UseInput;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return;
            if (Actor.OnKnockback()) return;

            int FinalValue = Velocity;

            if (FromConstants != null && FromConstants.Trim() != "")
            {
                bool hasKey = Actor.Data.CustomConstants.ContainsKey(FromConstants);
                if (GlobalFunctions.Assert(hasKey, "Constant value not found: " + FromConstants))
                    FinalValue = (int)Actor.Data.CustomConstants[FromConstants];
            }

            if (UseInput && Actor.Inputs != null)
            {
                int InputSide = 0;

                if (Actor.Inputs.IsBeingPressed(Actor.Inputs.CurrentHistory, PlayerInputs.UP))
                    InputSide = 1;
                else if (Actor.Inputs.IsBeingPressed(Actor.Inputs.CurrentHistory, PlayerInputs.DOWN))
                    InputSide = -1;

                if (UseInertia)
                    Actor.Body.AddVerticalAcceleration((int)FinalValue * InputSide);
                else
                    Actor.Body.SetVerticalVelocity((int)FinalValue * InputSide);
                
                return;
            }

            if (UseInertia)
                Actor.Body.AddVerticalAcceleration((int)FinalValue);
            else
                Actor.Body.SetVerticalVelocity((int)FinalValue);
        }
    }
}