using Godot;
using SakugaEngine.Resources;
using System;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FrameAnimator : Node
    {
        [Export] private AnimationPlayer[] players;
        [Export] private string[] prefix;
        [Export] public FighterState[] States;
        [Export] public Node3D OffsetNode;

        public int CurrentState;
        public int Frame;

        public void ViewAnimations()
        {
            if (GetCurrentState().animationSettings == null || GetCurrentState().animationSettings.Length == 0)
                return;

            AnimationSettings anim = GetCurrentAnimationSettings();
            if (anim == null) return;
            if (anim.SourceAnimation == "") return;

            int targetFrame = Frame - anim.AtFrame;
            if (anim.LimitRange)
            {
                targetFrame = (Frame - anim.AtFrame) + anim.AnimationRange.X;
                targetFrame = Mathf.Clamp(targetFrame, anim.AnimationRange.X, anim.AnimationRange.Y);
            }

            for (int a = 0; a < players.Length; a++)
            {
                players[a].Play(prefix[a] + anim.SourceAnimation);
                players[a].Seek(targetFrame / (float)Global.TicksPerSecond, true);
            }

            if (OffsetNode != null)
                OffsetNode.Position = anim.Offset;
        }

        public void PlayState(int state, bool reset = false)
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
            LoopState();
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

        private AnimationSettings GetCurrentAnimationSettings()
        {
            if (GetCurrentState().animationSettings.Length == 1)
                return GetCurrentState().animationSettings[0];

            int anim = 0;

            for (int i = 0; i < GetCurrentState().animationSettings.Length; i++)
            {
                int nextFrame = (i == GetCurrentState().animationSettings.Length - 1) ?
                                GetCurrentState().Duration - 1 :
                                GetCurrentState().animationSettings[i + 1].AtFrame - 1;
                
                if (Frame >= GetCurrentState().animationSettings[i].AtFrame && Frame <= nextFrame)
                    anim = i;
            }

            return GetCurrentState().animationSettings[anim];
        }
    }
}
