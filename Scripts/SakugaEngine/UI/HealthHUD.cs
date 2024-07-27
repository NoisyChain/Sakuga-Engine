using Godot;

namespace SakugaEngine.UI
{
    public partial class HealthHUD : Control
    {    
        [ExportCategory("Player 1")]
        [Export] private TextureRect P1Portrait;
        [Export] private TextureProgressBar P1Health;
        [Export] private RoundsCounter P1Rounds;
        [Export] private ComboCounter P1Combo;
        [Export] private Label P1Name;
        
        [ExportCategory("Player 2")]
        [Export] private TextureRect P2Portrait;
        [Export] private TextureProgressBar P2Health;
        [Export] private RoundsCounter P2Rounds;
        [Export] private ComboCounter P2Combo;
        [Export] private Label P2Name;

        [ExportCategory("Extra")]
        [Export] private Label Timer;

        public void Setup(FighterBody[] fighters)
        {
            P1Health.MaxValue = fighters[0].Variables.MaxHealth;
            P2Health.MaxValue = fighters[1].Variables.MaxHealth;
            
            if (fighters[0].Profile.Portrait != null)
            {
                P1Portrait.Texture = fighters[0].Profile.Portrait;
                P1Name.Text = fighters[0].Profile.ShortName;
            }

            if (fighters[1].Profile.Portrait != null)
            {
                P2Portrait.Texture = fighters[1].Profile.Portrait;
                P2Name.Text = fighters[1].Profile.ShortName;
            }
        }

        public void UpdateHealthBars(FighterBody[] fighters, int[] rounds)
        {
            P1Health.Value = fighters[0].Variables.CurrentHealth;
            P2Health.Value = fighters[1].Variables.CurrentHealth;
            

            P1Rounds.ShowRounds(rounds[0]);
            P2Rounds.ShowRounds(rounds[1]);

            P1Combo.Visible = fighters[1].Tracker.HitCombo > 0;
            P2Combo.Visible = fighters[0].Tracker.HitCombo > 0;

            P1Combo.UpdateCounter((int)fighters[1].HitStun.TimeLeft, fighters[1].Tracker);
            P2Combo.UpdateCounter((int)fighters[0].HitStun.TimeLeft, fighters[0].Tracker);
        }
        public void UpdateTimer(int timerValue)
        {
            Timer.Text = timerValue.ToString();
        }
    }
}
