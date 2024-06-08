using Godot;
using System;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FighterVariables : Node
    {
        [Export] public uint MaxHealth = 10000;
        [Export] public uint MaxSuperGauge = 10000;
        public uint CurrentHealth;
        public uint CurrentSuperGauge;

        public void Initialize()
        {
            CurrentHealth = MaxHealth;
            CurrentSuperGauge = 0;
        }
    }
}
