using Godot;
using System;

namespace SakugaEngine.UI
{
    public partial class RoundsCounter : Control
    {
        [Export] private int RoundsLimit = 2;
        [Export] public Control[] RoundViews;

        // Called when the node enters the scene tree for the first time.
        public void Setup()
        {
            for (int i = 0; i < RoundsLimit; i++)
            {
                RoundViews[i].Visible = false;
            }
        }

        public void ShowRounds(int roundsCount)
        {
            for (int i = 0; i < RoundsLimit; i++)
            {
                if (roundsCount - 1 == i)
                    RoundViews[i].Visible = true;
            }
        }
    }
}
