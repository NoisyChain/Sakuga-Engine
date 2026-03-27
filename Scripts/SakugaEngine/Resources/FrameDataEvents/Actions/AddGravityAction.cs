using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AddGravityAction : FrameDataAction
    {
        [Export] private bool SetValue;
        [Export] private bool UseDefault;
        [Export] private int Force;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return;

            if (SetValue) Actor.Body.CurrentGravity = UseDefault ? GlobalVariables.DefaultGravity : Force;
            Actor.Body.AddGravity();
        }
    }
}
