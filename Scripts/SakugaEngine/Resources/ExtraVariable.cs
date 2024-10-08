using Godot;
using System.IO;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ExtraVariable : Resource
    {
        [Export] public string Name;
        [Export] public int MaxValue;
        [Export] public int StartingValue;

        //Not working yet (gonna change some stuff)
        [Export] public ExtraVariableBehavior BehaviorOnStart;
        [Export] public ExtraVariableBehavior BehaviorOnHit;
        [Export] public ExtraVariableBehavior BehaviorOnDamage;
        [Export] public ExtraVariableBehavior BehaviorOnMoveChange;
        [Export] public ExtraVariableBehavior BehaviorOnFull;
        [Export] public ExtraVariableBehavior BehaviorOnEmpty;
    }
}
