using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SpawnObject : Resource
    {
        [Export] public PackedScene SpawnScene;
        [Export] public int Amount;
    }
}