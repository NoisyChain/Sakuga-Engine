using Godot;
using System;

namespace SakugaEngine.UI
{
    public partial class MainMenu : Control
    {
        [Export] private Button firstButton;

        public override void _Ready()
        {
            base._Ready();
            firstButton.GrabFocus();
        }
    }
}
