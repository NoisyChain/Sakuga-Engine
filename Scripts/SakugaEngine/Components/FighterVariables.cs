using Godot;
using System.IO;

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
        public uint LostHealth;
        public uint CurrentSuperGauge;
        public uint CurrentAttack;
        public uint CurrentDefense;
        public uint CurrentBaseDamageScaling;
        public uint CurrentCornerDamageScaling;
        public uint CurrentDamageProration = 100;
        public uint CurrentGravityProration = 100;
        public uint SuperArmor = 0;

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

        public void AddSuperArmor(uint value)
        {
            if (SuperArmor + value < 0) SuperArmor = 0;
            else SuperArmor += value;
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(CurrentHealth);
            bw.Write(LostHealth);
            bw.Write(CurrentSuperGauge);
            bw.Write(CurrentAttack);
            bw.Write(CurrentDefense);
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
            CurrentBaseDamageScaling = br.ReadUInt32();
            CurrentCornerDamageScaling = br.ReadUInt32();
            CurrentDamageProration = br.ReadUInt32();
            CurrentGravityProration = br.ReadUInt32();
            SuperArmor = br.ReadUInt32();
        }
    }
}
