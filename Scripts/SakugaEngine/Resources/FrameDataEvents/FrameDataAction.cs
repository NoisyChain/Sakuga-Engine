using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FrameDataAction : Resource
    {
        
        public virtual void Execute(ref SakugaActor Actor)
        {
            GD.Print($"Executing action at {Actor.Data.Profile.FighterName}");
        }
    }
}
