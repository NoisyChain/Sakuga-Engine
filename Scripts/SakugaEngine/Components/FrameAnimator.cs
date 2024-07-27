using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FrameAnimator : Node
    {
        [Export] private AnimationPlayer[] players;
        [Export] private string[] prefix;
        [Export] public FighterState[] States;

        public int CurrentState;
        public int Frame;

        public override void _Process(double delta)
        {
            for (int a = 0; a < players.Length; a++)
            {
                if (GetCurrentState().StateName != "")
                    players[a].Play(prefix[a] + GetCurrentState().StateName);
                
                players[a].Seek(Frame / (float)Global.TicksPerSecond, true);
            }
        }

        public void PlayState(int state, bool reset)
        {
            if (CurrentState != state)
            {
                CurrentState = state;
                Frame = 0;
                
            }
            else if (reset)
                Frame = 0;
        }

        public void RunState()
        {
            Frame++;
        }

        public void LoopState()
        {
            bool canLoop = GetCurrentState().Loop && GetCurrentState().LoopFrames.X >= 0 && GetCurrentState().LoopFrames.Y > 0;
            int frameLimit = canLoop ? GetCurrentState().LoopFrames.Y : GetCurrentState().Duration - 1;
            if (Frame > frameLimit)
            {
                if (canLoop)
                    Frame = GetCurrentState().LoopFrames.X;
                else
                    Frame = frameLimit;
            }
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(Frame);
            bw.Write(CurrentState);

        }

        public void Deserialize(BinaryReader br)
        {
            Frame = br.ReadInt32();
            CurrentState = br.ReadInt32();
        }

        public FighterState GetCurrentState() => States[CurrentState];
        public int StateType() => (int)GetCurrentState().Type;
    }
}
