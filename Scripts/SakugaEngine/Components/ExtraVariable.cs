using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class ExtraVariable : Node
    {
        [Export] private int MaxValue;
        [Export] private int StartingValue;
        [Export] private int StartingFactor;
        [Export] private Global.ExtraVariableMode StartingMode;

        [Export] private ExtraVariableBehavior BehaviorOnHit;
        [Export] private ExtraVariableBehavior BehaviorOnDamage;
        [Export] private ExtraVariableBehavior BehaviorOnMoveEnter;
        [Export] private ExtraVariableBehavior BehaviorOnMoveExit;
        [Export] private ExtraVariableBehavior BehaviorOnFull;
        [Export] private ExtraVariableBehavior BehaviorOnEmpty;
        [Export] private ExtraVariableBehavior BehaviorOnUse;

        public int CurrentValue;
        public int CurrentFactor;
        public byte CurrentMode;

        public void Initialize()
        {
            CurrentValue = StartingValue;
            CurrentFactor = StartingFactor;
            CurrentMode = (byte)StartingMode;
        }

        public void Add(int value)
        {
            CurrentValue += value;
            if (CurrentValue >= MaxValue)
                ChangeOnFull();
            
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }

        public void Subtract(int value)
        {
            CurrentValue -= value;
            if (CurrentValue <= 0)
                ChangeOnEmpty();
            
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }

        public void Set(int value)
        {
            CurrentValue = value;
            if (CurrentValue >= MaxValue)
                ChangeOnFull();
            else if (CurrentValue <= 0)
                ChangeOnEmpty();
            
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
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

        public void ChangeOnHit()
        {
            if (BehaviorOnHit == null) return;
            
            SetVariable(BehaviorOnHit);
        }

        public void ChangeOnDamage()
        {
            if (BehaviorOnDamage == null) return;
            
            SetVariable(BehaviorOnDamage);
        }

        public void ChangeOnMoveEnter()
        {
            if (BehaviorOnMoveEnter == null) return;
            
            SetVariable(BehaviorOnMoveEnter);
        }

        public void ChangeOnMoveExit()
        {
            if (BehaviorOnMoveExit == null) return;
            
            SetVariable(BehaviorOnMoveExit);
        }

        public void ChangeOnFull()
        {
            if (BehaviorOnFull == null) return;
            
            SetVariable(BehaviorOnFull);
        }

        public void ChangeOnEmpty()
        {
            if (BehaviorOnEmpty == null) return;
            
            SetVariable(BehaviorOnEmpty);
        }

        public void ChangeOnUse()
        {
            if (BehaviorOnUse == null) return;
            
            SetVariable(BehaviorOnUse);
        }

        private void SetVariable(ExtraVariableBehavior behavior)
        {
            if (behavior.SetValue)
            {
                if (behavior.IsRandom)
                    CurrentValue = Global.RNG.Next(behavior.Value, behavior.Range);
                else
                    CurrentValue = behavior.Value;
            }

            CurrentFactor = behavior.Factor;
            CurrentMode = (byte)behavior.Mode;
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
