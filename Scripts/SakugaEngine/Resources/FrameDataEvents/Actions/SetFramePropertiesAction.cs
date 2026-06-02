using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SetFramePropertiesAction : FrameDataAction
    {
        [Export] private FrameProperties Properties;
        [Export] private ParameterChange ChangeMode;
        public override void Execute(ref SakugaActor Actor)
        {
            switch (ChangeMode)
            {
                case ParameterChange.SET:
                    Actor.FrameProperties = Properties;
                    break;
                case ParameterChange.ADD:
                    Actor.FrameProperties |= Properties;
                    break;
                case ParameterChange.SUBTRACT:
                    Actor.FrameProperties &= ~Properties;
                    break;
            }
            
        }
    }
}
