using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SetTimerAction : FrameDataAction
    {
        [Export] private int ByIndex = -1;
        [Export] private string ByName;
        [Export(PropertyHint.Enum, "Play,Pause,Stop")] private int TimerMode;
        [Export] private uint Time = 0;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Parameters == null) return;
            FrameTimer timer = null;
            if (ByIndex >= 0) Actor.Parameters.GetTimer(ByIndex);
            else if (ByName != "") timer = Actor.Parameters.GetTimer(ByName);
            if (timer == null) return;

            timer.Start(Time);
        }
    }
}
