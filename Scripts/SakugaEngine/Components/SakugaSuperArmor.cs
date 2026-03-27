using Godot;
using System;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SakugaSuperArmor : Node
    {
        public int CurrentValue = 0;

        public void Initialize()
        {
            CurrentValue = 0;
        }

        public void SetArmor(int value)
        {
            CurrentValue = value;
            if (CurrentValue < 0) CurrentValue = 0;
        }

        public void AddArmor(int value)
        {
            CurrentValue += value;
            if (CurrentValue < 0) CurrentValue = 0;
        }

        public void RemoveArmor(int value)
        {
            CurrentValue -= value;
            if (CurrentValue < 0) CurrentValue = 0;
        }
    }
}
