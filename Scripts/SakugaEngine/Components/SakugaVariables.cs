using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SakugaVariables : Node
    {
        [ExportCategory("Main")]
        [Export] public int MaxHealth = 10000;
        [Export] public int MaxSuperGauge = 10000;

        [ExportCategory("Extra Variables")]
        [Export] public ExtraVariable[] ExtraVariables;
        private InternalExtraVariable[] internalExtraVariables;

        public int CurrentHealth;
        public int CurrentSuperGauge;
        public sbyte SuperArmor;

        public virtual void Initialize()
        {
            CurrentHealth = MaxHealth;
            CurrentSuperGauge = 0;
            SuperArmor = 0;

            if (ExtraVariables != null && ExtraVariables.Length > 0)
            {
                internalExtraVariables = new InternalExtraVariable[ExtraVariables.Length];
                for (int v = 0; v < internalExtraVariables.Length; v++)
                    internalExtraVariables[v].Initialize(ExtraVariables[v]);
            }
        }

        public void AddHealth(int value)
        {
            CurrentHealth += value;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        }

        public void RemoveHealth(int value)
        {
            CurrentHealth -= value;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        }

        public void AddSuperGauge(int value)
        {
            CurrentSuperGauge += value;
            CurrentSuperGauge = Mathf.Clamp(CurrentSuperGauge, 0, MaxSuperGauge);
        }

        public void RemoveSuperGauge(int value)
        {
            CurrentSuperGauge -= value;
            CurrentSuperGauge = Mathf.Clamp(CurrentSuperGauge, 0, MaxSuperGauge);
        }

        public void RemoveSuperArmor(sbyte value)
        {
            SuperArmor -= value;
        }

        public void TakeDamage(int damage, int meterGain, bool isKilingBlow)
        {
            AddSuperGauge(meterGain);
            if (!isKilingBlow && CurrentHealth - damage <= 1)
                CurrentHealth = 1;
            else
                RemoveHealth(damage);
        }

        public void ArmorDamage(sbyte damage, int healthDamage)
        {
            RemoveSuperArmor(damage);
            RemoveHealth(healthDamage);
        }

        public void UpdateExtraVariables()
        {
            if (!HasExtraVariables()) return;

            for (int v = 0; v < internalExtraVariables.Length; v++)
            {
                switch (internalExtraVariables[v].CurrentMode)
                {
                    case 0:
                        break;
                    case 1:
                        internalExtraVariables[v].Add(internalExtraVariables[v].CurrentFactor);
                        break;
                    case 2:
                        internalExtraVariables[v].Subtract(internalExtraVariables[v].CurrentFactor);
                        break;
                }
            }
        }

        public bool CompareExtraVariables(ExtraVariableCondition[] CompareTo)
        {
            if (CompareTo == null || CompareTo.Length <= 0) return true;

            for (int i = 0; i < CompareTo.Length; i++)
            {
                if (CompareTo[i].Value < 0) continue;

                if (internalExtraVariables[i].CurrentMode != (byte)CompareTo[i].Mode) return false;

                if (!internalExtraVariables[i].CompareValue(CompareTo[i].Value, (byte)CompareTo[i].CompareMode))
                    return false;
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
                        internalExtraVariables[i].Set(ToChange[i].ChangeValue);
                        break;
                    case 1:
                        internalExtraVariables[i].Add(ToChange[i].ChangeValue);
                        break;
                    case 2:
                        internalExtraVariables[i].Subtract(ToChange[i].ChangeValue);
                        break;
                }
            }
        }

        public void ExtraVariablesOnHit()
        {
            if (!HasExtraVariables()) return;

            for (int v = 0; v < internalExtraVariables.Length; v++)
                internalExtraVariables[v].ChangeOnHit();
        }

        public void ExtraVariablesOnDamage()
        {
            if (!HasExtraVariables()) return;

            for (int v = 0; v < internalExtraVariables.Length; v++)
                internalExtraVariables[v].ChangeOnDamage();
        }

        public void ExtraVariablesOnMoveEnter()
        {
            if (!HasExtraVariables()) return;

            for (int v = 0; v < internalExtraVariables.Length; v++)
                internalExtraVariables[v].ChangeOnMoveEnter();
        }

        public void ExtraVariablesOnMoveExit()
        {
            if (!HasExtraVariables()) return;

            for (int v = 0; v < internalExtraVariables.Length; v++)
                internalExtraVariables[v].ChangeOnMoveExit();
        }

        public void ExtraVariablesOnFull()
        {
            if (!HasExtraVariables()) return;

            for (int v = 0; v < internalExtraVariables.Length; v++)
                internalExtraVariables[v].ChangeOnFull();
        }

        public void ExtraVariablesOnEmpty()
        {
            if (!HasExtraVariables()) return;

            for (int v = 0; v < internalExtraVariables.Length; v++)
                internalExtraVariables[v].ChangeOnEmpty();
        }

        public bool HasExtraVariables() => internalExtraVariables != null && internalExtraVariables.Length > 0;

        public virtual void Serialize(BinaryWriter bw)
        {
            if (HasExtraVariables())
                for (int i = 0; i < internalExtraVariables.Length; i ++)
                    internalExtraVariables[i].Serialize(bw);
            
            bw.Write(CurrentHealth);
            bw.Write(CurrentSuperGauge);
            bw.Write(SuperArmor);
        }

        public virtual void Deserialize(BinaryReader br)
        {
            if (HasExtraVariables())
                for (int i = 0; i < internalExtraVariables.Length; i ++)
                    internalExtraVariables[i].Deserialize(br);
            
            CurrentHealth = br.ReadInt32();
            CurrentSuperGauge = br.ReadInt32();
            SuperArmor = br.ReadSByte();
        }
    }
}