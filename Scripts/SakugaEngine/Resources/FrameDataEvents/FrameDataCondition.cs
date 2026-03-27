using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FrameDataCondition : Resource
    {
        public virtual bool Check(ref SakugaActor Actor)
        {
            return Actor != null;
        }
    }
}
