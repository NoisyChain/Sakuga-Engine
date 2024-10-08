using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    public partial struct InternalExtraVariable
    {
        public int CurrentValue;
        public int CurrentFactor;
        public byte CurrentMode;
        private ExtraVariable Reference;

        public void Initialize(ExtraVariable reference)
        {
            Reference = reference;
            ChangeOnStart();
        }

        public void Add(int value)
        {
            CurrentValue += value;
            if (CurrentValue >= Reference.MaxValue)
                ChangeOnFull();
            
            CurrentValue = Mathf.Clamp(CurrentValue, 0, Reference.MaxValue);
            
        }

        public void Subtract(int value)
        {
            CurrentValue -= value;
            if (CurrentValue <= 0)
                ChangeOnEmpty();
            
            CurrentValue = Mathf.Clamp(CurrentValue, 0, Reference.MaxValue);
        }

        public void Set(int value)
        {
            CurrentValue = value;
            if (CurrentValue >= Reference.MaxValue)
                ChangeOnFull();
            else if (CurrentValue <= 0)
                ChangeOnEmpty();
            
            CurrentValue = Mathf.Clamp(CurrentValue, 0, Reference.MaxValue);
        }

        public bool CompareValue(uint CompareTo, byte mode)
        {
            switch (mode)
            {
                case 0:
                    return CurrentValue == CompareTo;
                case 1:
                    return CurrentValue > CompareTo;
                case 2:
                    return CurrentValue >= CompareTo;
                case 3:
                    return CurrentValue < CompareTo;
                case 4:
                    return CurrentValue <= CompareTo;
            }

            return false;
        }

        public void ChangeOnStart()
        {
            CurrentValue = 0;
            if (Reference.BehaviorOnStart == null)
            {
                CurrentFactor = 0;
                CurrentMode = 0;
            }
            else
            {
                CurrentFactor = Reference.BehaviorOnStart.Factor;
                CurrentMode = (byte)Reference.BehaviorOnStart.Mode;
            }
        }

        public void ChangeOnHit()
        {
            if (Reference.BehaviorOnHit == null) return;
            
            CurrentFactor = Reference.BehaviorOnHit.Factor;
            CurrentMode = (byte)Reference.BehaviorOnHit.Mode;
        }

        public void ChangeOnDamage()
        {
            if (Reference.BehaviorOnDamage == null) return;
            
            CurrentFactor = Reference.BehaviorOnDamage.Factor;
            CurrentMode = (byte)Reference.BehaviorOnDamage.Mode;
        }

        public void ChangeOnMove()
        {
            if (Reference.BehaviorOnMoveChange == null) return;
            
            CurrentFactor = Reference.BehaviorOnMoveChange.Factor;
            CurrentMode = (byte)Reference.BehaviorOnMoveChange.Mode;
        }

        public void ChangeOnFull()
        {
            if (Reference.BehaviorOnFull == null) return;
            
            CurrentFactor = Reference.BehaviorOnFull.Factor;
            CurrentMode = (byte)Reference.BehaviorOnFull.Mode;
        }

        public void ChangeOnEmpty()
        {
            if (Reference.BehaviorOnEmpty == null) return;
            
            CurrentFactor = Reference.BehaviorOnEmpty.Factor;
            CurrentMode = (byte)Reference.BehaviorOnEmpty.Mode;
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(CurrentValue);
            bw.Write(CurrentFactor);
            bw.Write(CurrentMode);
        }

        public void Deserialize(BinaryReader br)
        {
            CurrentValue = br.ReadInt32();
            CurrentFactor = br.ReadInt32();
            CurrentMode = br.ReadByte();
        }
    }
}
