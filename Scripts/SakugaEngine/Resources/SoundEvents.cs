using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SoundEvents : Resource
    {
        public int Frame;
        public Global.SoundType SoundType;
        public int Source;
        public int Index;
        public bool IsRandom;
        public int Range;
    }
}