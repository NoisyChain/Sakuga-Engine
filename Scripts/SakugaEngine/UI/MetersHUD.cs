using Godot;

namespace SakugaEngine.UI
{
    public partial class MetersHUD : Control
    {
        [ExportCategory("Components")]
        [Export] private TextureProgressBar P1Meter;
        [Export] private TextureProgressBar P2Meter;
        [Export] private Label P1TrainingInfo;
        [Export] private Label P2TrainingInfo;
        [Export] private InputHistory P1InputHistory;
        [Export] private InputHistory P2InputHistory;

        private int CurrentFrameAdvantage;

        public void Setup(SakugaActor[] fighters)
        {
            P1Meter.MaxValue = fighters[0].Data.MaxSuperGauge;
            P2Meter.MaxValue = fighters[1].Data.MaxSuperGauge;
        }

        public void UpdateMeters(SakugaActor[] fighters)
        {
            var P1SuperComp = fighters[0].Parameters.SuperGauge;
            var P2SuperComp = fighters[1].Parameters.SuperGauge;

            P1Meter.Value = P1SuperComp.CurrentValue;
            P2Meter.Value = P2SuperComp.CurrentValue;

            GetFrameAdvantage(fighters);

            if (P1InputHistory.Visible)  P1InputHistory.SetHistoryList(fighters[0].Inputs);
            if (P2InputHistory.Visible)  P2InputHistory.SetHistoryList(fighters[1].Inputs);

            if (P1TrainingInfo.Visible) P1TrainingInfo.Text = TrainingInfoText(fighters[0], fighters[1]);
            if (P2TrainingInfo.Visible) P2TrainingInfo.Text = TrainingInfoText(fighters[1], fighters[0]);
        }

        void GetFrameAdvantage(SakugaActor[] fighters)
        {
            for (int i = 0; i < fighters.Length; i++)
            {
                if (fighters[i].Parameters.Tracker.FrameAdvantage != 0)
                    CurrentFrameAdvantage = fighters[i].Parameters.Tracker.FrameAdvantage;
            }
        }

        private string TrainingInfoText(SakugaActor owner, SakugaActor reference)
        {
            string hitTypeText = "";

            switch (reference.Parameters.Tracker.LastHitType)
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
                    hitTypeText = "UNBL.";
                    break;
            }

            int finalFrameAdv = owner.Parameters.Tracker.FrameAdvantage;// != 0 ? CurrentFrameAdvantage : CurrentFrameAdvantage;

            string frameAdvantageInfo = finalFrameAdv >= 0 ?
                    ("+" + finalFrameAdv) : "" + finalFrameAdv;

            string frameAdvText = "(" + frameAdvantageInfo + ")";

            return reference.Parameters.Tracker.LastDamage + "\n" +
                    reference.Parameters.Tracker.CurrentCombo + "\n" +
                    reference.Parameters.Tracker.HighestCombo + "\n" +
                    hitTypeText + "\n" +
                    reference.Parameters.Prorations.CurrentDamageScaling + "%\n" +
                    owner.Parameters.Tracker.FrameData + frameAdvText;
        }
    }
}