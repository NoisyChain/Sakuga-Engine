using Godot;
using SakugaEngine.Resources;
using System;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FrameAnimator : Node
    {
        [Export] private AnimationPlayer player;
        public FighterState ActiveState;
        public int Frame;

        public override void _Process(double delta)
        {
            player.Seek(Frame / (float)Global.TicksPerSecond, true);
        }

        public void PlayState(FighterState state, bool reset)
        {
            if (ActiveState != state)
            {
                ActiveState = state;
                Frame = 0;
                if (ActiveState.StateName != "") player.Play(ActiveState.StateName);
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
            bool canLoop = ActiveState.Loop && ActiveState.LoopStartFrame >= 0 && ActiveState.LoopEndFrame > 0;
            int frameLimit = canLoop ? ActiveState.LoopEndFrame : ActiveState.Duration - 1;
            if (Frame > frameLimit)
            {
                if (canLoop)
                    Frame = ActiveState.LoopStartFrame;
                else
                    Frame = frameLimit;
            }
        }
    }
}
