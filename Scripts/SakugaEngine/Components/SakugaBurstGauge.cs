using Godot;
using SakugaEngine.Resources;

namespace SakugaEngine
{
    [GlobalClass]
	public partial class SakugaBurstGauge : Node
	{
		[Export] private bool UseDataContainer;
        [Export] private bool CanReset = true;
        [Export] public int MaxValue;
		[Export] public int StartingValue;
		[Export] public int FillRate;

        public int CurrentValue;
        public bool Locked;

        public bool _IsFull => CurrentValue >= MaxValue;
        public bool _IsHalfFull => CurrentValue >= MaxValue / 2;
        

        public void Initialize(DataContainer data)
        {
            if (UseDataContainer)
            {
                MaxValue = data.MaxBurst;
				StartingValue = data.StartingBurst;
				FillRate = data.BurstFillRate;
            }
            
            CurrentValue = StartingValue;
            Locked = false;
        }

        public void Reset()
        {
            if (!CanReset) return;

            CurrentValue = StartingValue;
            Locked = false;
        }

        public void Tick()
        {
            if (CurrentValue < MaxValue) AddBurstGauge(FillRate);
        }

        public void SetBurstGauge(int value)
        {
            CurrentValue = value;
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }

        public void AddBurstGauge(int value)
        {
            CurrentValue += value;
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }

        public void SpendBurstGauge(int value)
        {
            CurrentValue -= value;
            CurrentValue = Mathf.Clamp(CurrentValue, 0, MaxValue);
        }
	}
}
