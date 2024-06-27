using Godot;
using SakugaEngine;

namespace SakugaEngine.UI
{
    public partial class ComboCounter : Control
    {
        [Export] private Color InvalidHitColor = new Color(0, 0, 0);
        Color DefaultColor;
        public TextureProgressBar StunBar;
        public Label ComboCount;
        public override void _Ready()
        {
            StunBar = GetNode<TextureProgressBar>("StunBar");
            ComboCount = GetNode<Label>("ComboCount");
            DefaultColor = StunBar.TintProgress;
        }

        public void UpdateCounter(int stunValue, CombatTracker tracker)
        {
            StunBar.TintProgress = tracker.invalidHit ? InvalidHitColor : DefaultColor;
            StunBar.Value = stunValue;
            ComboCount.Text = tracker.HitCombo.ToString();
        }
    }
}
