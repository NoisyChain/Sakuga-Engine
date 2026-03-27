using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
    public partial class HitboxState : Resource
    {
        [Export] public int AtFrame;
        [Export] public Global.AnimationStage animationStage;
        [Export] public HitboxSettings HitboxData;
    }
}
