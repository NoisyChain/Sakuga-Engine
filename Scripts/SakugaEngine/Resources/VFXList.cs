using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class VFXList : Resource
    {
        [Export] public PackedScene[] VFXObjects;
    }
}
