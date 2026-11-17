using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AddGravityAction : FrameDataAction
    {
        [Export] private bool SetValue;
        [Export] private bool UseDefault;
        [Export] private string FromConstants;
        [Export] private int Force;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Body == null) return;

            int FinalValue = Force;

            if (FromConstants != null && FromConstants.Trim() != "")
            {
                bool hasKey = Actor.Data.CustomConstants.ContainsKey(FromConstants);
                if (GlobalFunctions.Assert(hasKey, "Constant value not found: " + FromConstants))
                    FinalValue = (int)Actor.Data.CustomConstants[FromConstants];
            }

            if (SetValue) Actor.Body.CurrentGravity = UseDefault ? GlobalVariables.DefaultGravity : FinalValue;
            Actor.Body.AddGravity();
        }
    }
}
