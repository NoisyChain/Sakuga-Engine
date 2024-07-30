using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ExtraVariable : Resource
    {
        [Export] public string Name;
        [Export] public uint MaxValue;
        [Export] public uint StartingValue;

        //Not working yet (gonna change some stuff)
        [Export] public ExtraVariableBehaviour BehaviourOnStart;
        [Export] public ExtraVariableBehaviour BehaviourOnHit;
        [Export] public ExtraVariableBehaviour BehaviourOnDamage;
        [Export] public ExtraVariableBehaviour BehaviourOnChange;
        [Export] public ExtraVariableBehaviour BehaviourOnFull;
        [Export] public ExtraVariableBehaviour BehaviourOnEmpty;
    }
}
