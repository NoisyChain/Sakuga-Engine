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
        [Export] private TextureProgressBar P1RecoveryHealth;
        [Export] private TextureProgressBar P1Burst;
        [Export] private RoundsCounter P1Rounds;
        [Export] private ComboCounter P1Combo;
        [Export] private Label P1Name;
        [Export] private HitNotificationController P1Notif;
        
        [ExportCategory("Player 2")]
        [Export] private TextureRect P2Portrait;
        [Export] private TextureProgressBar P2Health;
        [Export] private TextureProgressBar P2LostHealth;
        [Export] private TextureProgressBar P2RecoveryHealth;
        [Export] private TextureProgressBar P2Burst;
        [Export] private RoundsCounter P2Rounds;
        [Export] private ComboCounter P2Combo;
        [Export] private Label P2Name;
        [Export] private HitNotificationController P2Notif;

        [ExportCategory("Extra")]
        [Export] private Label Timer;
        [Export] private Label P1Debug;
        [Export] private Label P2Debug;

        public void Setup(SakugaActor[] fighters)
        {
            P1Health.MaxValue = fighters[0].Data.MaxHealth;
            P2Health.MaxValue = fighters[1].Data.MaxHealth;
            P1LostHealth.MaxValue = fighters[0].Data.MaxHealth;
            P2LostHealth.MaxValue = fighters[1].Data.MaxHealth;
            P1RecoveryHealth.MaxValue = fighters[0].Data.MaxHealth;
            P2RecoveryHealth.MaxValue = fighters[1].Data.MaxHealth;

            P1Burst.MaxValue = fighters[0].Data.MaxBurst;
            P2Burst.MaxValue = fighters[1].Data.MaxBurst;
            
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
            var P1HealthComp = fighters[0].Parameters.Health;
            var P2HealthComp = fighters[1].Parameters.Health;
            var P1BurstComp = fighters[0].Parameters.BurstGauge;
            var P2BurstComp = fighters[1].Parameters.BurstGauge;

            P1Health.Value = P1HealthComp.CurrentValue;
            P2Health.Value = P2HealthComp.CurrentValue;
            P1LostHealth.Value = P1HealthComp.LostValue;
            P2LostHealth.Value = P2HealthComp.LostValue;
            P1RecoveryHealth.Value = P1HealthComp.CurrentValue + P1HealthComp.RecoverableValue;
            P2RecoveryHealth.Value = P2HealthComp.CurrentValue + P2HealthComp.RecoverableValue;
            P1Burst.Value = P1BurstComp.CurrentValue;
            P2Burst.Value = P2BurstComp.CurrentValue;

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
            if (P1Debug.Visible) P1Debug.Text = fighters[0].DebugInfo();
            if (P2Debug.Visible) P2Debug.Text = fighters[1].DebugInfo();
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

        public void UpdateHitNotifications(HitNotifications[] notifs)
        {
            P1Notif.View(notifs[0].Elements);
            P2Notif.View(notifs[1].Elements);
        }
    }
}
