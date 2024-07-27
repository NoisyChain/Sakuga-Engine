using Godot;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FighterVariables : Node
    {
        [Export] public uint MaxHealth = 10000;
        [Export] public uint MaxSuperGauge = 10000;
        [Export] public uint BaseAttack = 100;
        [Export] public uint BaseDefense = 100;
        [Export] public uint BaseMinDamageScaling = 35;
        [Export] public uint BaseMaxDamageScaling = 100;
        [Export] public uint CornerMinDamageScaling = 45;
        [Export] public uint CornerMaxDamageScaling = 120;
        
        public uint CurrentHealth;
        public uint LostHealth;
        public uint CurrentSuperGauge;
        public uint CurrentAttack;
        public uint CurrentDefense;
        public uint CurrentDamageScaling;
        public uint CurrentBaseDamageScaling;
        public uint CurrentCornerDamageScaling;
        public uint CurrentDamageProration;
        public uint CurrentGravityProration;
        public uint SuperArmor;

        public void Initialize()
        {
            CurrentHealth = MaxHealth;
            LostHealth = CurrentHealth;
            CurrentSuperGauge = 0;

            CurrentAttack = BaseAttack;
            CurrentDefense = BaseDefense;

            CurrentBaseDamageScaling = BaseMaxDamageScaling;
            CurrentCornerDamageScaling = CornerMaxDamageScaling;
            CurrentDamageScaling = CurrentBaseDamageScaling;

            CurrentDamageProration = 100;
            CurrentGravityProration = 100;
            SuperArmor = 0;
        }

        public void AddHealth(uint value)
        {
            if (CurrentHealth + value > MaxHealth) CurrentHealth = MaxHealth;
            else CurrentHealth += value;
        }

        public void RemoveHealth(uint value)
        {
            if (CurrentHealth - value < 0) CurrentHealth = 0;
            else CurrentHealth -= value;
        }

        public void AddSuperGauge(uint value)
        {
            if (CurrentSuperGauge + value > MaxSuperGauge) CurrentSuperGauge = MaxSuperGauge;
            else CurrentSuperGauge += value;
        }

        public void RemoveSuperGauge(uint value)
        {
            if (CurrentSuperGauge - value < 0) CurrentSuperGauge = 0;
            else CurrentSuperGauge -= value;
        }

        public void RemoveSuperArmor(uint value)
        {
            if (SuperArmor - value < 0) SuperArmor = 0;
            else SuperArmor -= value;
        }

        public void TakeDamage(uint damage, uint meterGain, uint damageScalingSubtract, bool isKilingBlow)
        {
            AddSuperGauge(meterGain);
            if (!isKilingBlow && CurrentHealth - damage <= 1)
                CurrentHealth = 1;
            else
                RemoveHealth(damage);
            if (damage > 0) 
            {
                RemoveDamageScaling(damageScalingSubtract);
            }
        }

        public void ArmorDamage(uint damage, uint healthDamage)
        {
            RemoveSuperArmor(damage);
            RemoveHealth(healthDamage);
        }

        public void RemoveDamageScaling(uint value)
        {
            if (CurrentBaseDamageScaling - value < BaseMinDamageScaling)
                CurrentBaseDamageScaling = BaseMinDamageScaling;
            else CurrentBaseDamageScaling -= value;

            if (CurrentCornerDamageScaling - value < CornerMinDamageScaling)
                CurrentCornerDamageScaling = CornerMinDamageScaling;
            else CurrentCornerDamageScaling -= value;
        }

        public void ResetDamageStatus()
        {
            CurrentBaseDamageScaling = BaseMaxDamageScaling;
            CurrentCornerDamageScaling = CornerMaxDamageScaling;
            CurrentDamageProration = 100;
            CurrentGravityProration = 100;
        }

        public void UpdateLostHealth()
        {
            if (LostHealth > CurrentHealth)
                LostHealth -= MaxHealth / 200;
            else if (LostHealth < CurrentHealth)
                LostHealth = CurrentHealth;
        }

        public void CalculateDamageScaling(bool changeCondition)
        {
            if (changeCondition)
                CurrentDamageScaling = CurrentCornerDamageScaling;
            else
                CurrentDamageScaling = CurrentBaseDamageScaling;
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(CurrentHealth);
            bw.Write(LostHealth);
            bw.Write(CurrentSuperGauge);
            bw.Write(CurrentAttack);
            bw.Write(CurrentDefense);
            bw.Write(CurrentDamageScaling);
            bw.Write(CurrentBaseDamageScaling);
            bw.Write(CurrentCornerDamageScaling);
            bw.Write(CurrentDamageProration);
            bw.Write(CurrentGravityProration);
            bw.Write(SuperArmor);
        }

        public void Deserialize(BinaryReader br)
        {
            CurrentHealth = br.ReadUInt32();
            LostHealth = br.ReadUInt32();
            CurrentSuperGauge = br.ReadUInt32();
            CurrentAttack = br.ReadUInt32();
            CurrentDefense = br.ReadUInt32();
            CurrentDamageScaling = br.ReadUInt32();
            CurrentBaseDamageScaling = br.ReadUInt32();
            CurrentCornerDamageScaling = br.ReadUInt32();
            CurrentDamageProration = br.ReadUInt32();
            CurrentGravityProration = br.ReadUInt32();
            SuperArmor = br.ReadUInt32();
        }
    }
}
