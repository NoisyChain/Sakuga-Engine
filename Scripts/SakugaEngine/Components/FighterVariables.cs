using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FighterVariables : SakugaVariables
    {
        [Export] public ushort BaseAttack = 100;
        [Export] public ushort BaseDefense = 100;
        [Export] public ushort BaseMinDamageScaling = 35;
        [Export] public ushort BaseMaxDamageScaling = 100;
        [Export] public ushort CornerMinDamageScaling = 45;
        [Export] public ushort CornerMaxDamageScaling = 120;
        
        public ushort CurrentAttack;
        public ushort CurrentDefense;
        public ushort CurrentDamageScaling;
        public ushort CurrentBaseDamageScaling;
        public ushort CurrentCornerDamageScaling;
        public ushort CurrentDamageProration;
        public ushort CurrentGravityProration;
        
        public int LostHealth;

        public override void Initialize()
        {
            base.Initialize();
            LostHealth = CurrentHealth;

            CurrentAttack = BaseAttack;
            CurrentDefense = BaseDefense;

            CurrentBaseDamageScaling = BaseMaxDamageScaling;
            CurrentCornerDamageScaling = CornerMaxDamageScaling;
            CurrentDamageScaling = CurrentBaseDamageScaling;

            CurrentDamageProration = 100;
            CurrentGravityProration = 100;
        }

        public override void TakeDamage(int damage, int meterGain, bool isKilingBlow)
        {
            base.TakeDamage(damage, meterGain, isKilingBlow);
            if (CurrentHealth == 0)
                LostHealth = 0;
        }

        public void RemoveDamageScaling(ushort value)
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
            UpdateLostHealth();
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

        public int CalculateCompleteDamage(int damage, int attackValue)
        {
            var damageFactor = attackValue - (CurrentDefense - 100);
            var scaledDamage = damage * CurrentDamageScaling / 100;
            return scaledDamage * damageFactor / 100;
        }

        public override void Serialize(BinaryWriter bw)
        {
            base.Serialize(bw);

            bw.Write(LostHealth);
            bw.Write(CurrentAttack);
            bw.Write(CurrentDefense);
            bw.Write(CurrentDamageScaling);
            bw.Write(CurrentBaseDamageScaling);
            bw.Write(CurrentCornerDamageScaling);
            bw.Write(CurrentDamageProration);
            bw.Write(CurrentGravityProration);
        }

        public override void Deserialize(BinaryReader br)
        {
            base.Deserialize(br);

            LostHealth = br.ReadInt32();
            CurrentAttack = br.ReadUInt16();
            CurrentDefense = br.ReadUInt16();
            CurrentDamageScaling = br.ReadUInt16();
            CurrentBaseDamageScaling = br.ReadUInt16();
            CurrentCornerDamageScaling = br.ReadUInt16();
            CurrentDamageProration = br.ReadUInt16();
            CurrentGravityProration = br.ReadUInt16();
        }
    }
}
