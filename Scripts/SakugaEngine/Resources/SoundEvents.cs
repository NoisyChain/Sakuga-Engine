using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SoundEvents : Resource
    {
        [Export] public int Frame;
        [Export] public Global.SoundType SoundType;
        [Export] public int Source;
        [Export] public int Index;
        [Export] public bool IsRandom;
        [Export] public int Range;
    }
}