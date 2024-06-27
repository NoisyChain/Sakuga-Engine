using Godot;
using System;

namespace SakugaEngine.UI
{
    [Tool]
    public partial class FadeScreen : ColorRect
    {
        [Export(PropertyHint.Range, "0, 100")] public int FadeIntensity;

        public override void _Process(double delta)
        {
            float alpha = (float)FadeIntensity / 100f;
            Color = new Color(Color.R, Color.G, Color.B, alpha);
        }
    }
}
