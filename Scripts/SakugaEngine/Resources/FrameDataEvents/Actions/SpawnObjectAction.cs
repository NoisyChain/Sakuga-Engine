using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SpawnObjectAction : FrameDataAction
    {
        [Export] private ObjectType ObjectType;
        [Export] private Vector2I TargetPosition = Vector2I.Zero;
        [Export] private RelativeTo xRelativeTo, yRelativeTo;
        [Export] private int Index;
        [Export] private bool IsRandom;
        [Export] private int Range;
        [Export] private string Key;
        [Export] private int FromExtraVariable = -1;
        [Export] private bool AttachToParent; // Only works with VFX lel
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Pool == null) return;
            
            Vector2I dst = Actor.GenerateTargetPosition(TargetPosition, Index, xRelativeTo, yRelativeTo);
                            
            int ind = IsRandom ? RNG.Next(Index, Range) : Index;
            if (FromExtraVariable >= 0)
            {
                ind = Actor.Parameters.Variables[FromExtraVariable].CurrentValue;
                Actor.Parameters.Variables[FromExtraVariable].ChangeBehavior(CustomVariableBehaviorTarget.ON_USE);
            }
            switch (ObjectType)
            {
                case ObjectType.SPAWNABLE:
                    if (ind >= 0)
                        Actor.Pool.SpawnObject(ind, dst);
                    else if (Key != "")
                        Actor.Pool.SpawnObject(Key, dst);
                    break;
                case ObjectType.VFX:
                    if (ind >= 0)
                        Actor.Pool.SpawnVFX(ind, dst, Actor.InputSide, AttachToParent);
                    else if (Key != "")
                        Actor.Pool.SpawnVFX(ind, dst, Actor.InputSide, AttachToParent);
                    break;
            }
        }
    }
}
