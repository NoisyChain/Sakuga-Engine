using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SpawnsList : Resource
    {
        [Export] public SpawnObject[] SpawnObjects;
    }
}
