using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class HitboxState : Resource
    {
        [Export] public Global.AnimationStage animationStage;
        [Export] public int Frame;
        [Export] public int HitboxIndex;
    }
}
