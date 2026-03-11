using Godot;
using SakugaEngine;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class DataContainer : Resource
    {
        [Export] public FighterProfile Profile;
        [Export] public HitboxSettings[] Hitboxes;
        [Export] public FighterState[] States;
        [Export] public FighterStance[] Stances;

        [Export] public int MaxHealth = 10000;
        [Export] public int MaxSuperGauge = 10000;
        [Export] public ushort BaseAttack = 100;
        [Export] public ushort BaseDefense = 100;

        [Export] public AIData _aiData;

        [Export] public FighterIntro[] Intros;
        [Export] public FighterOutro[] Outros;
        [Export] public FighterIntro[] VictoryPoses;
        [Export] public int DefeatPose;
    }
}
