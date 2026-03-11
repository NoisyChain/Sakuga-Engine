using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FrameAnimator : Node
    {
        private SakugaActor _owner;
        [Export] private AnimationPlayer[] players;
        [Export] private string[] prefix;
        [Export] public Node3D OffsetNode;

        public int CurrentState;
        public int CurrentStateFrame;

        private int targetFrame = 0;

        public void Initialize(SakugaActor owner)
        {
            _owner = owner;
        }

        public void ViewAnimations(AnimationSettings anim, int Frame)
        {
            if (anim == null)
                return;
            
            if (anim == null) return;
            if (anim.SourceAnimation == "") return;

            int modTarget = (Frame - anim.AtFrame) % anim.FrameStepping;

            if (anim.LimitRange)
            {
                targetFrame = (Frame - anim.AtFrame) + anim.AnimationRange.X;
                targetFrame = Mathf.Clamp(targetFrame, anim.AnimationRange.X, anim.AnimationRange.Y);
                if (anim.AnimationRange.Y > anim.AnimationRange.X)
                    targetFrame -= modTarget;
            }
            else
            {
                targetFrame = Frame - anim.AtFrame;
                targetFrame -= modTarget;
            }

            for (int a = 0; a < players.Length; a++)
            {
                string prfx = "";
                if (prefix != null && prefix.Length > a) prfx = prefix[a];
                players[a].SpeedScale = anim.Speed;
                players[a].Play(prfx + anim.SourceAnimation);
                players[a].Seek(targetFrame / (float)Global.TicksPerSecond, true);
            }

            if (OffsetNode != null)
                OffsetNode.Position = anim.Offset;
        }

        public void PlayState(int state, bool reset = false)
        {
            if (state < 0) return;
            
            if (CurrentState != state)
            {
                CurrentState = state;
                CurrentStateFrame = 0;

            }
            else if (reset)
                CurrentStateFrame = 0;
            
            _owner.CancelConditions = Global.CancelCondition.WHIFF_CANCEL | Global.CancelCondition.KARA_CANCEL;
        }

        public void RunState()
        {
            CurrentStateFrame++;
            if (CurrentStateFrame >= Global.KaraCancelWindow) _owner.CancelConditions &= ~Global.CancelCondition.KARA_CANCEL;
            LoopState();
        }

        public void LoopState()
        {
            bool canLoop = GetCurrentState().Loop && GetCurrentState().LoopFrames.X >= 0 && GetCurrentState().LoopFrames.Y > 0;
            int frameLimit = canLoop ? GetCurrentState().LoopFrames.Y : GetCurrentState().Duration - 1;
            if (CurrentStateFrame > frameLimit)
            {
                if (canLoop)
                    CurrentStateFrame = GetCurrentState().LoopFrames.X;
                else
                    CurrentStateFrame = GetCurrentState().Duration;
            }
        }

        public FighterState GetCurrentState() => _owner.Data.States[CurrentState];
        public Global.StateType CurrentStateType() => GetCurrentState().Type;
        public bool StateEnded() => CurrentStateFrame >= GetCurrentState().Duration;

        public AnimationSettings GetCurrentAnimationSettings()
        {
            if (GetCurrentState().animationSettings == null || GetCurrentState().animationSettings.Length <= 0) return null;
            if (GetCurrentState().animationSettings.Length == 1)
                return GetCurrentState().animationSettings[0];

            int anim = 0;

            for (int i = 0; i < GetCurrentState().animationSettings.Length; i++)
            {
                int nextFrame = (i == GetCurrentState().animationSettings.Length - 1) ?
                                GetCurrentState().Duration - 1 :
                                GetCurrentState().animationSettings[i + 1].AtFrame - 1;
                
                if (CurrentStateFrame >= GetCurrentState().animationSettings[i].AtFrame && CurrentStateFrame <= nextFrame)
                    anim = i;
            }

            return GetCurrentState().animationSettings[anim];
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(CurrentState);
            bw.Write(CurrentStateFrame);
        }

        public void Deserialize(BinaryReader br)
        {
            CurrentState = br.ReadInt32();
            CurrentStateFrame = br.ReadInt32();
        }
    }
}
