using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AnimationEvents : Resource
    {    
        [Export] public int Frame;
        [Export] public Global.EventType Type;
        [Export] public Global.ObjectType Object;
        [Export] public Global.RelativeTo xRelativeTo, yRelativeTo;
        [Export] public int Index;
        [Export] public bool IsRandom;
        [Export] public int Range;
        [Export] public int Value;
        [Export] public bool SetActive;
        [Export] public Vector2I TargetPosition = Vector2I.Zero;
    }
}
