using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CheckInputCondition : FrameDataCondition
    {
        [Export] private MotionInputs Input;
        [Export] private bool Not;
        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.Inputs == null) return false;
            
            if (Not) return !Actor.Inputs.CheckMotionInputs(Input, Actor.InputSide);
            return Actor.Inputs.CheckMotionInputs(Input, Actor.InputSide);
        }
    }
}
