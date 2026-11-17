using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class MoveXAction : FrameDataAction
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

                if (Actor.Inputs.IsBeingPressed(Actor.Inputs.CurrentHistory, PlayerInputs.RIGHT))
                    InputSide = 1;
                else if (Actor.Inputs.IsBeingPressed(Actor.Inputs.CurrentHistory, PlayerInputs.LEFT))
                    InputSide = -1;

                if (UseInertia)
                    Actor.Body.AddLateralAcceleration((int)FinalValue * InputSide);
                else
                    Actor.Body.SetLateralVelocity((int)FinalValue * InputSide);
                
                return;
            }

            if (UseInertia)
                Actor.Body.AddLateralAcceleration((int)FinalValue * Actor.InputSide());
            else
                Actor.Body.SetLateralVelocity((int)FinalValue * Actor.InputSide());
        }
    }
}