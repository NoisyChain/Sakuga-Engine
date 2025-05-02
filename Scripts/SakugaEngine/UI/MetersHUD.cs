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

        private int CurrentFrameAdvantage;

        /*public override void _Ready()
        {
            P1Meter = GetNode<TextureProgressBar>("Meters/P1Meter");
            P2Meter = GetNode<TextureProgressBar>("Meters/P2Meter");
            P1TrainingInfo = GetNode<Label>("TrainingInfo/P1Info/Information");
            P2TrainingInfo = GetNode<Label>("TrainingInfo/P2Info/Information");
        }*/

        public void Setup(SakugaFighter[] fighters)
        {
            P1Meter.MaxValue = fighters[0].Variables.MaxSuperGauge;
            P2Meter.MaxValue = fighters[1].Variables.MaxSuperGauge;
        }

        public void UpdateMeters(SakugaFighter[] fighters)
        {
            P1Meter.Value = fighters[0].Variables.CurrentSuperGauge;
            P2Meter.Value = fighters[1].Variables.CurrentSuperGauge;

            GetFrameAdvantage(fighters);

            P1InputHistory.SetHistoryList(fighters[0].Inputs);
            P2InputHistory.SetHistoryList(fighters[1].Inputs);

            P1TrainingInfo.Text = TrainingInfoText(fighters[0], fighters[1]);
            P2TrainingInfo.Text = TrainingInfoText(fighters[1], fighters[0]);
        }

        void GetFrameAdvantage(SakugaFighter[] fighters)
        {
            for (int i = 0; i < fighters.Length; i++)
            {
                if (fighters[i].Tracker.FrameAdvantage != 0)
                    CurrentFrameAdvantage = fighters[i].Tracker.FrameAdvantage;
            }
        }

        private string TrainingInfoText(SakugaFighter owner, SakugaFighter reference)
        {
            string hitTypeText = "";

            switch (reference.Tracker.LastHitType)
            {
                case 0:
                    hitTypeText = "HIGH";
                    break;
                case 1:
                    hitTypeText = "MID";
                    break;
                case 2:
                    hitTypeText = "LOW";
                    break;
                case 3:
                    hitTypeText = "UNBLOCKABLE";
                    break;
            }

            int finalFrameAdv = owner.Tracker.FrameAdvantage;// != 0 ? CurrentFrameAdvantage : CurrentFrameAdvantage;

            string frameAdvantageInfo = finalFrameAdv >= 0 ?
                    ("+" + finalFrameAdv) : "" + finalFrameAdv;

            string frameAdvText = "(" + frameAdvantageInfo + ")";

            FighterVariables vars = reference.Variables as FighterVariables;

            return reference.Tracker.LastDamage + "\n" +
                    reference.Tracker.CurrentCombo + "\n" +
                    reference.Tracker.HighestCombo + "\n" +
                    hitTypeText + "\n" +
                    vars.CurrentDamageScaling + "%\n" +
                    owner.Tracker.FrameData + frameAdvText;
        }
    }
}