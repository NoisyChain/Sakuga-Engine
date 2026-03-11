using Godot;
using SakugaEngine.Resources;

namespace SakugaEngine.UI
{
    public partial class MatchCardsController : Control
    {
        [Export] private AnimationPlayer animator;
        [Export] private Label RoundLabel;
        [Export] private Label KO_Label;
        [Export] private string RoundName = "Round";
        [Export] private string[] KO_Names = ["K.O.", "PERFECT!", "Double K.O.", "Time Up!"];

        /*[ExportGroup("Announcer")]
        [Export] private SoundsList AnnouncerVoiceLines;
        [Export] private SoundQueue AnnouncerSound;*/

        [ExportGroup("Animation References")]
        [Export] private MatchAnimation[] Animations;
        [Export] private int CatchPhraseAnimation = 0;
        [Export] private int RoundStartAnimation = 1;
        [Export] private int KOAnimationDefault = 2;
        [Export] private int KOAnimationP1 = 2;
        [Export] private int KOAnimationP2 = 3;
        [Export] private int WinAnimationP1 = 4;
        [Export] private int WinAnimationP2 = 5;
        [Export] private int DrawAnimation = 6;

        public int CurrentAnimation = -1;
        public int Frame;

        public void Render()
        {
            if (CurrentAnimation < 0)
            {
                animator.Play("RESET");
                return;
            }

            animator.Play(GetCurrentAnimation().Name);
            animator.Seek(Frame / (float)Global.TicksPerSecond, true);
        }

        public void Run()
        {
            if (CurrentAnimation < 0) return;

            if (Frame < GetCurrentAnimation().Duration)Frame++;
            else
            {
                Frame = 0;
                CurrentAnimation = -1;
            }
        }

        public void PlayCatchPhraseAnimation()
        {
            //animator.Play("RESET");
            CurrentAnimation = CatchPhraseAnimation;
            Frame = 0;
            //PlayAnnouncerLine(GetCurrentAnimation().AnnouncerLineIndex);
        }

        public void PlayRoundStartAnimation(int currentRound, bool final)
        {
            //animator.Play("RESET");
            if (final)
                SetRoundLabel($"Final {RoundName}");
            else
                SetRoundLabel($"{RoundName} {currentRound + 1}");
            
            CurrentAnimation = RoundStartAnimation;
            Frame = 0;
            //PlayAnnouncerLine(GetCurrentAnimation().AnnouncerLineIndex);
        }

        public void PlayKnockoutAnimation(int playerIndex, int koType)
        {
            //animator.Play("RESET");
            SetKOLabel(KO_Names[koType]);
            switch (playerIndex)
            {
                case 0:
                    CurrentAnimation = KOAnimationP1;
                    break;
                case 1:
                    CurrentAnimation = KOAnimationP2;
                    break;
                default:
                    CurrentAnimation = KOAnimationDefault;
                    break;
            }
            Frame = 0;
            //PlayAnnouncerLine(GetCurrentAnimation().AnnouncerLineIndex);
        }

        private void SetRoundLabel(string text)
        {
            RoundLabel.SetDeferred(Label.PropertyName.Text, text);
        }

        private void SetKOLabel(string text)
        {
            KO_Label.SetDeferred(Label.PropertyName.Text, text);
        }

        public void PlayPlayerWinAnimation(int playerIndex)
        {
            //animator.Play("RESET");
            switch (playerIndex)
            {
                case 0:
                    CurrentAnimation = WinAnimationP1;
                    break;
                case 1:
                    CurrentAnimation = WinAnimationP2;
                    break;
                default:
                    CurrentAnimation = DrawAnimation;
                    break;
            }
            Frame = 0;
            //PlayAnnouncerLine(GetCurrentAnimation().AnnouncerLineIndex);
        }

        /*public void PlayAnnouncerLine(int index)
        {
            if (AnnouncerVoiceLines == null) return;
            if (index < 0) return;

            AnnouncerSound.QueueSound(AnnouncerVoiceLines.Sounds[index]);
        }*/

        public MatchAnimation GetCurrentAnimation() => Animations[CurrentAnimation];
        public bool AnimationEnded() => Frame >= GetCurrentAnimation().Duration;
        public uint GetStateDelay() => (uint)(GetCurrentAnimation().Duration - GetCurrentAnimation().DelaySubtract);
    }
}
