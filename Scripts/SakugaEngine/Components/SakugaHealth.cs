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

        public void Initialize(DataContainer data)
        {
            if (UseDataContainer)
                MaxValue = data.MaxHealth;
            
            CurrentValue = MaxValue;
            LostValue = CurrentValue;
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
                LostValue -= MaxValue / 200;
        }
    }
}
