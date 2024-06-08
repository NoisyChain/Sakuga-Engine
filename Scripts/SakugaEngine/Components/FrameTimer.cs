using Godot;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FrameTimer : Node
    {
        [Export] public uint WaitTime = 0;
        public uint TimeLeft = 0;

        public void Start(uint startingTime = 0)
        {
            TimeLeft = startingTime == 0 ? WaitTime : startingTime;
        }

        public void Run()
        {
            if (TimeLeft > 0) TimeLeft--;
        }

        public bool IsRunning() => TimeLeft > 0;
    }
}
