using Godot;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FrameTimer : Node
    {
        [Export] public uint WaitTime = 0;
        public uint TimeLeft = 0;
        private bool Paused;

        public void Start(uint startingTime = 0)
        {
            Paused = false;
            TimeLeft = startingTime == 0 ? WaitTime : startingTime;
        }

        public void Run() { if (TimeLeft > 0 && !Paused) TimeLeft--; }
        public void Pause() { Paused = true; }
        public void Resume() { Paused = false; }
        public void Stop() { TimeLeft =  0; Paused = false; }

        public bool IsRunning() => TimeLeft > 0 && !Paused;
        public bool IsPaused() => Paused;
    }
}
