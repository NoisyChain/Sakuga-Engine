using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SpawnVFXAction : FrameDataAction
    {
        [Export] private Vector2I TargetPosition = Vector2I.Zero;
        [Export] private RelativeTo xRelativeTo, yRelativeTo;
        [Export] private int Index;
        [Export] private bool IsRandom;
        [Export] private int Range;
        [Export] private string Key;
        [Export] private int FromExtraVariable = -1;
        [Export] private bool AttachToParent;
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
            
            if (ind >= 0)
                Actor.Pool.SpawnVFX(ind, dst, Actor.InputSide, AttachToParent);
            else if (Key != "")
                Actor.Pool.SpawnVFX(ind, dst, Actor.InputSide, AttachToParent);
        }
    }
}
