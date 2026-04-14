using Godot;
using SakugaEngine.Resources;
using SakugaEngine.Global;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class CustomVariable : Node
    {
        [Export] public bool Reset;
        [Export] private int MaxValue;
        [Export] private int StartingValue;
        [Export] private int StartingFactor;
        [Export] private CustomVariableMode StartingMode;
        [Export] private CustomVariableBehavior[] Behaviors;

        public int CurrentValue;
        public int CurrentFactor;
        public CustomVariableMode CurrentMode;

        public void Initialize()
        {
            CurrentValue = StartingValue;
            CurrentFactor = StartingFactor;
            CurrentMode = StartingMode;
        }

        public void Add(int value)
        {
            CurrentValue += value;
            if (CurrentValue >= MaxValue)
                ChangeBehavior(CustomVariableBehaviorTarget.ON_FULL);
            
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }

        public void Subtract(int value)
        {
            CurrentValue -= value;
            if (CurrentValue <= 0)
                ChangeBehavior(CustomVariableBehaviorTarget.ON_EMPTY);
            
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }

        public void Set(int value)
        {
            CurrentValue = value;
            if (CurrentValue >= MaxValue)
                ChangeBehavior(CustomVariableBehaviorTarget.ON_FULL);
            else if (CurrentValue <= 0)
                ChangeBehavior(CustomVariableBehaviorTarget.ON_EMPTY);
            
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }

        public bool CompareValue(int CompareTo, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.EQUAL:
                    return CurrentValue == CompareTo;
                case CompareMode.DIFFERENT:
                    return CurrentValue != CompareTo;
                case CompareMode.GREATER:
                    return CurrentValue > CompareTo;
                case CompareMode.GREATER_EQUAL:
                    return CurrentValue >= CompareTo;
                case CompareMode.LESS:
                    return CurrentValue < CompareTo;
                case CompareMode.LESS_EQUAL:
                    return CurrentValue <= CompareTo;
            }

            return false;
        }

        public void ChangeBehavior(CustomVariableBehaviorTarget CompareTarget)
        {
            if (Behaviors == null || Behaviors.Length == 0) return;
            
            foreach (CustomVariableBehavior behavior in Behaviors)
            {
                if (behavior.Target != CompareTarget) continue;

                SetVariable(behavior);
                break;
            }
        }

        private void SetVariable(CustomVariableBehavior behavior)
        {
            if (behavior.SetValue)
            {
                if (behavior.IsRandom)
                    CurrentValue = RNG.Next(behavior.Value, behavior.Range);
                else
                    CurrentValue = behavior.Value;
            }

            CurrentFactor = behavior.Factor;
            CurrentMode = behavior.Mode;
        }

        public bool CompareVariable(CustomVariableCondition CompareTo)
        {
            if (CompareTo == null) return false;

            if (CompareTo.Value < 0) return false;

            if (CurrentMode != CompareTo.Mode) return false;

            if (!CompareValue(CompareTo.Value, CompareTo.CompareMode)) return false;

            return true;
        }

        public void ChangeVariable(CustomVariableChange ToChange)
        {
            if (ToChange == null) return;

                int val = ToChange.Value;
                if (ToChange.IsRandom)
                    val = RNG.Next(ToChange.Value, ToChange.Range);

                switch ((byte)ToChange.Mode)
                {
                    case 0:
                        Set(val);
                        break;
                    case 1:
                        Add(val);
                        break;
                    case 2:
                        Subtract(val);
                        break;
                }
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(CurrentValue);
            bw.Write(CurrentFactor);
            bw.Write((byte)CurrentMode);
        }

        public void Deserialize(BinaryReader br)
        {
            CurrentValue = br.ReadInt32();
            CurrentFactor = br.ReadInt32();
            CurrentMode = (CustomVariableMode)br.ReadByte();
        }
    }
}
