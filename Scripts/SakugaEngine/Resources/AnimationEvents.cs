using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class AnimationEvents : Resource
    {    
        public enum EventType{SpawnObject, ForceMoveCancel, AnimationDamage, DettachThrow, ForceChangeSide, Teleport, SetArmor}
        public enum RelativeTo{World, Self, Opponent}
        [Export] public int Frame;
        [Export] public EventType eventType;
        [Export] public int Index;
        [Export] public int Value;
        [Export] public bool SetActive;
        [Export] public Vector2I Origin = Vector2I.Zero;
        [Export] public RelativeTo xRelativeTo, yRelativeTo;
    }
}
