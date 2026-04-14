using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SetKnockoutStateAction : FrameDataAction
    {
        [Export] private bool Not;

        public override void Execute(ref SakugaActor Actor)
        {
            GD.Print("Should be dead for good...");
        }
    }
}
