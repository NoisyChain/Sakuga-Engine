using Godot;
using SakugaEngine.Game;
using SakugaEngine.Global;
using SakugaEngine.Resources;

namespace SakugaEngine.UI
{
    public partial class MatchCardsController : Control
    {
        [Export] private AnimationPlayer animator;
        [Export] private Label RoundLabel;
        [Export] private Label KO_Label;
        [Export] private string RoundName = "Round";
        [Export] private RoundNamingOrder RoundNaming;
        [Export] private string[] KO_Names = ["K.O.", "PERFECT!", "Double K.O.", "Time Up!"];

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
            else if (CurrentAnimation == RoundStartAnimation && Frame == 100) AudioManager.Instance.PlayAnnouncerClip(GetCurrentAnimation().AnnouncerLineIndex + 6);

            animator.Play(GetCurrentAnimation().Name);
            animator.Seek(Frame / (float)GlobalVariables.TicksPerSecond, true);
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
            AudioManager.Instance.PlayAnnouncerClip(GetCurrentAnimation().AnnouncerLineIndex);
        }

        public void PlayRoundStartAnimation(int currentRound, bool final)
        {
            CurrentAnimation = RoundStartAnimation;
            Frame = 0;

            if (final)
            {
                SetRoundLabel($"Final {RoundName}");
                AudioManager.Instance.PlayAnnouncerClip(GetCurrentAnimation().AnnouncerLineIndex + 5);
            }
            else
            {
                switch (RoundNaming)
                {
                    case RoundNamingOrder.N:
                        SetRoundLabel($"{RoundName} {currentRound + 1}");
                        break;
                    case RoundNamingOrder.Nth:
                        string placing = "";
                        switch (currentRound)
                        {
                            case 0:
                                placing = "st";
                                break;
                            case 1:
                                placing = "nd";
                                break;
                            case 2:
                                placing = "rd";
                                break;
                            default:
                                placing = "th";
                                break;
                        }
                        SetRoundLabel($"{currentRound + 1}{placing} {RoundName}");
                        break;
                }
                AudioManager.Instance.PlayAnnouncerClip(GetCurrentAnimation().AnnouncerLineIndex + currentRound);
            }
        }

        public void PlayKnockoutAnimation(int playerIndex, int koType)
        {
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
            AudioManager.Instance.PlayAnnouncerClip(GetCurrentAnimation().AnnouncerLineIndex + koType);
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
            switch (playerIndex)
            {
                case 0:
                    CurrentAnimation = WinAnimationP1;
                    AudioManager.Instance.PlayAnnouncerClip(GetCurrentAnimation().AnnouncerLineIndex + GameManager.Instance.Match.P1SelectedCharacter);
                    break;
                case 1:
                    CurrentAnimation = WinAnimationP2;
                    AudioManager.Instance.PlayAnnouncerClip(GetCurrentAnimation().AnnouncerLineIndex + GameManager.Instance.Match.P2SelectedCharacter);
                    break;
                default:
                    CurrentAnimation = DrawAnimation;
                    AudioManager.Instance.PlayAnnouncerClip(GetCurrentAnimation().AnnouncerLineIndex);
                    break;
            }
            Frame = 0;
        }

        public MatchAnimation GetCurrentAnimation() => Animations[CurrentAnimation];
        public bool AnimationEnded() => Frame >= GetCurrentAnimation().Duration;
        public uint GetStateDelay() => (uint)(GetCurrentAnimation().Duration - GetCurrentAnimation().DelaySubtract);
    }
}
