using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SakugaHealth : Node
    {
        [Export] private bool UseDataContainer;
        [Export] public int MaxValue;
        public int CurrentValue;
        public int LostValue;
        public int RecoverableValue;

        public int DangerousHealth => (MaxValue * 20) / 100;

        public void Initialize(DataContainer data)
        {
            if (UseDataContainer)
                MaxValue = data.MaxHealth;
            
            CurrentValue = MaxValue;
            LostValue = CurrentValue;
            RecoverableValue = 0;
        }

        public void Reset()
        {
            CurrentValue = MaxValue;
            LostValue = CurrentValue;
            RecoverableValue = 0;
        }

        public void Tick()
        {
            UpdateLostHealth();
        }

        public void SetHealth(int value)
        {
            CurrentValue = value;
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);

            if (CurrentValue == 0 || CurrentValue == MaxValue)
                LostValue = CurrentValue;
        }

        public void AddHealth(int value)
        {
            CurrentValue += value;
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);

            if (CurrentValue == 0 || CurrentValue == MaxValue)
                LostValue = CurrentValue;
        }

        public void RemoveHealth(int value)
        {
            CurrentValue -= value;
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);

            if (CurrentValue == 0 || CurrentValue == MaxValue)
                LostValue = CurrentValue;
        }

        public void UpdateLostHealth()
        {
            if (LostValue > CurrentValue)
                LostValue -= MaxValue / 100;
            else if (LostValue < CurrentValue)
                LostValue = CurrentValue;
        }

        public void SetRecoveryHealth(int value)
        {
            RecoverableValue = value;
        }

        public void RecoverHealth()
        {
            if (RecoverableValue == 0) return;

            AddHealth(1);
            RecoverableValue--;
        }
    }
}
