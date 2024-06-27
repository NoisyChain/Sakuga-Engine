using Godot;

namespace SakugaEngine.UI
{
    public partial class MetersHUD : Control
    {
        [Export] private TextureProgressBar P1Meter;
        [Export] private TextureProgressBar P2Meter;
        [Export] private Label P1TrainingInfo;
        [Export] private Label P2TrainingInfo;
        [Export] private InputHistory P1InputHistory;
        [Export] private InputHistory P2InputHistory;

        /*public override void _Ready()
        {
            P1Meter = GetNode<TextureProgressBar>("Meters/P1Meter");
            P2Meter = GetNode<TextureProgressBar>("Meters/P2Meter");
            P1TrainingInfo = GetNode<Label>("TrainingInfo/P1Info/Information");
            P2TrainingInfo = GetNode<Label>("TrainingInfo/P2Info/Information");
        }*/

        public void Setup(FighterBody[] fighters)
        {
            P1Meter.MaxValue = fighters[0].Variables.MaxSuperGauge;
            P2Meter.MaxValue = fighters[1].Variables.MaxSuperGauge;
        }

        public void UpdateMeters(FighterBody[] fighters)
        {
            P1Meter.Value = fighters[0].Variables.CurrentSuperGauge;
            P2Meter.Value = fighters[1].Variables.CurrentSuperGauge;

            P1TrainingInfo.Text = TrainingInfoText(fighters[1]);
            P2TrainingInfo.Text = TrainingInfoText(fighters[0]);

            P1InputHistory.SetHistoryList(fighters[0].Inputs);
            P2InputHistory.SetHistoryList(fighters[1].Inputs);
        }

        private string TrainingInfoText(FighterBody reference)
        {
            /*string hitTypeText = "";

            switch (reference.tracker.LastHitType)
            {
                case 0:
                    hitTypeText = "LOW";
                    break;
                case 1:
                    hitTypeText = "MID";
                    break;
                case 2:
                    hitTypeText = "HIGH";
                    break;
            }

            string frameAdvantageInfo = reference.tracker.FrameAdvantage >= 0 ? 
            "+" + reference.tracker.FrameAdvantage : reference.tracker.FrameAdvantage.ToString();

            return reference.tracker.LastDamage + "\n" +
                    reference.tracker.CurrentCombo + "\n" +
                    reference.tracker.HighestCombo + "\n" +
                    hitTypeText + "\n" +
                    reference.tracker.FrameData + "("+ frameAdvantageInfo + ")";*/
            return "";
        }
    }
}