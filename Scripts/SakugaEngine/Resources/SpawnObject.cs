using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SpawnObject : Resource
    {
        [Export] public string Key;
        [Export] public PackedScene Instance;
        [Export] public int PoolSize;
    }
}