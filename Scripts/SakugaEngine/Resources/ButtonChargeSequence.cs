using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ButtonChargeSequence : Resource
    {
        [Export] public Vector2I Threshold;
        [Export] public int SequenceMove;
    }
}
