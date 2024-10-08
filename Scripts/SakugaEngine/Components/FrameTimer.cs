using Godot;
using System.IO;

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

        public bool IsRunning() => TimeLeft > 0;
        public bool IsCounting() => TimeLeft > 0 && !Paused;
        public bool IsPaused() => Paused;

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(WaitTime);
            bw.Write(TimeLeft);
            bw.Write(Paused);
        }

        public void Deserialize(BinaryReader br)
        {
            WaitTime = br.ReadUInt32();
            TimeLeft = br.ReadUInt32();
            Paused = br.ReadBoolean();
        }
    }
}
