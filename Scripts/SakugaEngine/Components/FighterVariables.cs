using Godot;
using SakugaEngine.Resources;
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
        [Export] public ExtraVariable[] ExtraVariables;
        
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
        public InternalExtraVariable[] ExtraVars;

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

            if (ExtraVariables != null && ExtraVariables.Length > 0)
            {
                ExtraVars = new InternalExtraVariable[ExtraVariables.Length];
            
                for (int v = 0; v < ExtraVars.Length; v++)
                {
                    ExtraVars[v] = new InternalExtraVariable
                    {
                        Value = ExtraVariables[v].StartingValue,
                        MaxValue = ExtraVariables[v].MaxValue,
                        Factor = 0,
                        Mode = (byte)Global.ExtraVariableMode.IDLE
                    };
                    ExtraVariablesOnStart(v);
                }
            }
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

        public void UpdateExtraVariables()
        {
            if (!HasExtraVariables()) return;
            
            for (int v = 0; v < ExtraVars.Length; v++)
                ExtraVars[v].Update();
        }

        public bool CompareExtraVariables(ExtraVariableCondition[] CompareTo)
        {
            if (CompareTo == null || CompareTo.Length <= 0) return true;

            for (int i = 0; i < CompareTo.Length; i++)
            {
                if (CompareTo[i].Value < 0) continue;

                if (ExtraVars[i].Mode != (byte)CompareTo[i].Mode) return false;

                if (!ExtraVars[i].CompareValue(CompareTo[i].Value, (byte)CompareTo[i].CompareMode))
                {
                    return false;
                }
            }
            return true;
        }

        public void SetExtraVariables(ExtraVariableChange[] ToChange)
        {
            if (ToChange == null || ToChange.Length <= 0) return;

            for (int i = 0; i < ToChange.Length; i++)
            {
                if (ToChange[i] == null) continue;

                switch ((byte)ToChange[i].ChangeMode)
                {
                    case 0:
                        ExtraVars[i].Set(ToChange[i].ChangeValue);
                        break;
                    case 1:
                        ExtraVars[i].Add(ToChange[i].ChangeValue);
                        break;
                    case 2:
                        ExtraVars[i].Subtract(ToChange[i].ChangeValue);
                        break;
                }

                ExtraVariablesOnChange(i);
            }
        }

        public void ExtraVariablesOnStart(int value)
        {
            if (!HasExtraVariables()) return;
            if (ExtraVariables[value].BehaviourOnStart == null) return;

            ExtraVars[value].Factor = ExtraVariables[value].BehaviourOnStart.Factor;
            ExtraVars[value].Mode = (byte)ExtraVariables[value].BehaviourOnStart.Mode;
        }

        public void ExtraVariablesOnHit(int value)
        {
            if (!HasExtraVariables()) return;
            if (ExtraVariables[value].BehaviourOnHit == null) return;

            ExtraVars[value].Factor = ExtraVariables[value].BehaviourOnHit.Factor;
            ExtraVars[value].Mode = (byte)ExtraVariables[value].BehaviourOnHit.Mode;
        }

        public void ExtraVariablesOnDamage(int value)
        {
            if (!HasExtraVariables()) return;
            if (ExtraVariables[value].BehaviourOnDamage == null) return;

            ExtraVars[value].Factor = ExtraVariables[value].BehaviourOnDamage.Factor;
            ExtraVars[value].Mode = (byte)ExtraVariables[value].BehaviourOnDamage.Mode;
        }

        public void ExtraVariablesOnChange(int value)
        {
            if (!HasExtraVariables()) return;
            if (ExtraVariables[value].BehaviourOnChange == null) return;

            ExtraVars[value].Factor = ExtraVariables[value].BehaviourOnChange.Factor;
            ExtraVars[value].Mode = (byte)ExtraVariables[value].BehaviourOnChange.Mode;
        }

        public void ExtraVariablesOnFull(int value)
        {
            if (!HasExtraVariables()) return;
            if (ExtraVariables[value].BehaviourOnFull == null) return;

            ExtraVars[value].Factor = ExtraVariables[value].BehaviourOnFull.Factor;
            ExtraVars[value].Mode = (byte)ExtraVariables[value].BehaviourOnFull.Mode;
        }

        public void ExtraVariablesOnEmpty(int value)
        {
            if (!HasExtraVariables()) return;
            if (ExtraVariables[value].BehaviourOnEmpty == null) return;

            ExtraVars[value].Factor = ExtraVariables[value].BehaviourOnEmpty.Factor;
            ExtraVars[value].Mode = (byte)ExtraVariables[value].BehaviourOnEmpty.Mode;
        }

        public bool HasExtraVariables() => ExtraVars != null && ExtraVars.Length > 0;

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

            if (HasExtraVariables())
                for (int i = 0; i < ExtraVars.Length; i ++)
                    ExtraVars[i].Serialize(bw);
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

            if (HasExtraVariables())
                for (int i = 0; i < ExtraVars.Length; i ++)
                    ExtraVars[i].Deserialize(br);
        }
    }

    public struct InternalExtraVariable
    {
        public uint Value;
        public uint MaxValue;
        public uint Factor;
        public byte Mode;

        public void Update()
        {
            switch (Mode)
            {
                case 0:
                    break;
                case 1:
                    Add(Factor);
                    break;
                case 2:
                    Subtract(Factor);
                    break;
            }
        }

        public void Add(uint value)
        {
            if (Value + value > MaxValue) Value = MaxValue;
            else Value += value;
        }

        public void Subtract(uint value)
        {
            if (Value - value < 0) Value = 0;
            else Value -= value;
        }

        public void Set(uint value)
        {
            if (value > MaxValue)
                Value = MaxValue;
            else if (value < 0)
                Value = 0;
            else
                Value = value;
        }

        public bool CompareValue(uint CompareTo, byte mode)
        {
            switch (mode)
            {
                case 0:
                    return Value == CompareTo;
                case 1:
                    return Value > CompareTo;
                case 2:
                    return Value >= CompareTo;
                case 3:
                    return Value < CompareTo;
                case 4:
                    return Value <= CompareTo;
            }

            return false;
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(Value);
            bw.Write(Factor);
            bw.Write(Mode);
        }

        public void Deserialize(BinaryReader br)
        {
            Value = br.ReadUInt32();
            Factor = br.ReadUInt32();
            Mode = br.ReadByte();
        }
    }
}
