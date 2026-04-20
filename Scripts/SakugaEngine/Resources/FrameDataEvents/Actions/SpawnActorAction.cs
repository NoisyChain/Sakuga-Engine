using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SpawnActorAction : FrameDataAction
    {
        [Export] private Vector2I TargetPosition = Vector2I.Zero;
        [Export] private RelativeTo xRelativeTo, yRelativeTo;
        [Export] private int Index;
        [Export] private bool IsRandom;
        [Export] private int Range;
        [Export] private string Key;
        [Export] private int FromExtraVariable = -1;
        [Export] private int InitialState;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Pool == null) return;
            
            Vector2I origin = Actor.GenerateTargetPosition(TargetPosition, Index, xRelativeTo, yRelativeTo);
                            
            int ind = IsRandom ? RNG.Next(Index, Range) : Index;
            if (FromExtraVariable >= 0)
            {
                ind = Actor.Parameters.Variables[FromExtraVariable].CurrentValue;
                Actor.Parameters.Variables[FromExtraVariable].ChangeBehavior(CustomVariableBehaviorTarget.ON_USE);
            }
            
            if (ind >= 0)
                Actor.Pool.SpawnObject(ind, InitialState, origin);
            else if (Key != "")
                Actor.Pool.SpawnObject(Key, InitialState, origin);
        }
    }
}
