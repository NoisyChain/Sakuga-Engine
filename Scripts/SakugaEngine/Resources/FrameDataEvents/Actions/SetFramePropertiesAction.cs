using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SetFramePropertiesAction : FrameDataAction
    {
        [Export] private Global.FrameProperties Properties;
        public override void Execute(ref SakugaActor Actor)
        {
            Actor.FrameProperties = Properties;
        }
    }
}
