using Godot;
using System;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FighterVariables : Node
    {
        [Export] public uint MaxHealth = 10000;
        [Export] public uint MaxSuperGauge = 10000;
        [Export] public uint BaseAttack;
        [Export] public uint BaseDefense;
        [Export] public uint BaseMinDamageScaling;
        [Export] public uint BaseMaxDamageScaling;
        [Export] public uint CornerMinDamageScaling;
        [Export] public uint CornerMaxDamageScaling;
        
        public uint CurrentHealth;
        public uint CurrentSuperGauge;
        public uint CurrentAttack;
        public uint CurrentDefense;
        public uint CurrentDamageScaling;
        public uint CurrentDamageProration = 100;
        public uint CurrentGravityProration = 100;

        public void Initialize()
        {
            CurrentHealth = MaxHealth;
            CurrentSuperGauge = 0;
        }

        public void AddHealth(uint value)
        {
            if (CurrentHealth + value < 0) CurrentHealth = 0;
            else if (CurrentHealth + value > MaxHealth) CurrentHealth = MaxHealth;
            else CurrentHealth += value;
        }

        public void AddSuperGauge(uint value)
        {
            if (CurrentSuperGauge + value < 0) CurrentSuperGauge = 0;
            else if (CurrentSuperGauge + value > MaxSuperGauge) CurrentSuperGauge = MaxSuperGauge;
            else CurrentSuperGauge += value;
        }
    }
}
