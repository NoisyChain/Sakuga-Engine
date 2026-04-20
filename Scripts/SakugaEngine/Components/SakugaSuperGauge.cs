using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SakugaSuperGauge : Node
    {
        [Export] private bool UseDataContainer;
        [Export] private bool StartFull = false;
        [Export] private bool CanReset = true;
        [Export] public int MaxValue;
        public int CurrentValue;
        

        public void Initialize(DataContainer data)
        {
            if (UseDataContainer)
            {
                MaxValue = data.MaxSuperGauge;
            }
            
            CurrentValue = StartFull ? MaxValue : 0;
        }

        public void Reset()
        {
            if (!CanReset) return;

            CurrentValue = StartFull ? MaxValue : 0;
        }

        public void Tick()
        {
            if (CurrentValue > MaxValue) CurrentValue = MaxValue;
        }

        public void SetSuperGauge(int value)
        {
            CurrentValue = value;
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }

        public void AddSuperGauge(int value)
        {
            CurrentValue += value;
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }

        public void SpendSuperGauge(int value)
        {
            CurrentValue -= value;
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }
    }
}
