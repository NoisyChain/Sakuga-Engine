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
        [Export] private TextureRect Star;
        [Export] private bool inverseRotation;
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
            if (Star != null)
            {
                if (tracker.invalidHit) Star.Rotation -= inverseRotation ? -0.01f : 0.01f;
                else if (stunValue > 0)
                    Star.Rotation += (inverseRotation ? -0.03f : 0.03f) * ((float)tracker.HitCombo * 0.5f);
            }
        }
    }
}
