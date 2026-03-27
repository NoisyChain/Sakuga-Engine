using System;
using Godot;
using SakugaEngine.Global;

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

        public void Setup(SakugaActor[] fighters)
        {
            P1Health.MaxValue = fighters[0].Data.MaxHealth;
            P2Health.MaxValue = fighters[1].Data.MaxHealth;
            
            if (fighters[0].Data.Profile.Portrait != null)
            {
                P1Portrait.Texture = fighters[0].Data.Profile.Portrait;
                P1Name.Text = fighters[0].Data.Profile.ShortName;
            }

            if (fighters[1].Data.Profile.Portrait != null)
            {
                P2Portrait.Texture = fighters[1].Data.Profile.Portrait;
                P2Name.Text = fighters[1].Data.Profile.ShortName;
            }

            P1Rounds.Setup();
            P2Rounds.Setup();
        }

        public void UpdateHealthBars(SakugaActor[] fighters, GameMonitor monitor)
        {
            P1Health.Value = fighters[0].Parameters.Health.CurrentValue;
            P2Health.Value = fighters[1].Parameters.Health.CurrentValue;
            P1LostHealth.Value = fighters[0].Parameters.Health.LostValue;
            P2LostHealth.Value = fighters[1].Parameters.Health.LostValue;

            UpdateTimer(monitor);

            P1Rounds.ShowRounds(monitor.VictoryCounter[0]);
            P2Rounds.ShowRounds(monitor.VictoryCounter[1]);

            P1Combo.Visible = fighters[1].Parameters.Tracker.HitCombo > 0;
            P2Combo.Visible = fighters[0].Parameters.Tracker.HitCombo > 0;

            P1Combo.UpdateCounter((int)fighters[1].Hitstun.TimeLeft, fighters[1].Parameters.Tracker);
            P2Combo.UpdateCounter((int)fighters[0].Hitstun.TimeLeft, fighters[0].Parameters.Tracker);
            UpdateDebug(fighters);
        }

        public void UpdateDebug(SakugaActor[] fighters)
        {
            P1Debug.Text = fighters[0].DebugInfo();
            P2Debug.Text = fighters[1].DebugInfo();
        }

        public void UpdateTimer(GameMonitor monitor)
        {
            if (monitor.ClockLimit < 0)
            {
                Timer.Text = "--";
                return;
            }
            int time = monitor.Clock / GlobalVariables.TicksPerSecond;
            time = Mathf.Clamp(time, 0, monitor.ClockLimit);
            Timer.Text = time.ToString();
        }
    }
}
