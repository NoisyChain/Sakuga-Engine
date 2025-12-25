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
        [Export] public SpawnsList SpawnablesList;
        [Export] public SpawnsList VFXList;
        [Export] public SoundsList SFXList;
        [Export] public SoundsList VoiceLines;

        [Export] public int MaxHealth = 10000;
        [Export] public int MaxSuperGauge = 10000;
        [Export] public ushort BaseAttack = 100;
        [Export] public ushort BaseDefense = 100;

        [Export] public AIData _aiData;
    }
}
