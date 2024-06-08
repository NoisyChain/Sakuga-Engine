using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ButtonChargeSequence : Resource
    {
        [Export] public int MinimumValue;
        [Export] public int MaximumValue;
        [Export] public int SequenceMove;
    }
}
