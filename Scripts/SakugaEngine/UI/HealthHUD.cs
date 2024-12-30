using System;
using Godot;

namespace SakugaEngine.UI
{
    public partial class HealthHUD : Control
    {    
        [ExportCategory("Player 1")]
        [Export] private TextureRect P1Portrait;
        [Export] private TextureProgressBar P1Health;
        [Export] private TextureProgressBar P1LostHealth;
        [Export] private RoundsCounter P1Rounds;
        [Export] private ComboCounter P1Combo;
        [Export] private Label P1Name;
        
        [ExportCategory("Player 2")]
        [Export] private TextureRect P2Portrait;
        [Export] private TextureProgressBar P2Health;
        [Export] private TextureProgressBar P2LostHealth;
        [Export] private RoundsCounter P2Rounds;
        [Export] private ComboCounter P2Combo;
        [Export] private Label P2Name;

        [ExportCategory("Extra")]
        [Export] private Label Timer;
        [Export] private Label P1Debug;
        [Export] private Label P2Debug;

        public void Setup(SakugaFighter[] fighters)
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

            P1Rounds.Setup();
            P2Rounds.Setup();
        }

        public void UpdateHealthBars(SakugaFighter[] fighters, GameMonitor monitor)
        {
            P1Health.Value = fighters[0].Variables.CurrentHealth;
            P2Health.Value = fighters[1].Variables.CurrentHealth;
            P1LostHealth.Value = fighters[0].FighterVars.LostHealth;
            P2LostHealth.Value = fighters[1].FighterVars.LostHealth;

            UpdateTimer(monitor);

            P1Rounds.ShowRounds(monitor.VictoryCounter[0]);
            P2Rounds.ShowRounds(monitor.VictoryCounter[1]);

            P1Combo.Visible = fighters[1].Tracker.HitCombo > 0;
            P2Combo.Visible = fighters[0].Tracker.HitCombo > 0;

            P1Combo.UpdateCounter((int)fighters[1].HitStun.TimeLeft, fighters[1].Tracker);
            P2Combo.UpdateCounter((int)fighters[0].HitStun.TimeLeft, fighters[0].Tracker);
            UpdateDebug(fighters);
        }

        public void UpdateDebug(SakugaFighter[] fighters)
        {
            P1Debug.Text = fighters[0].DebugInfo();
            P2Debug.Text = fighters[1].DebugInfo();
        }

        public void UpdateTimer(GameMonitor monitor)
        {
            int time = (monitor.Clock / Global.TicksPerSecond) + 1;
            time = Mathf.Clamp(time, 0, monitor.ClockLimit);
            Timer.Text = time.ToString();
        }
    }
}
