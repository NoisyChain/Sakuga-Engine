using Godot;
using System;

namespace SakugaEngine.UI
{
    public partial class RoundsCounter : HBoxContainer
    {
        [Export] private int RoundsLimit = 2;
        public TextureRect[] RoundViews;

        // Called when the node enters the scene tree for the first time.
        public void Setup()
        {
            RoundViews = new TextureRect[RoundsLimit];
            for (int i = 0; i < RoundsLimit; i++)
            {
                RoundViews[i] = GetNode<TextureRect>("Round" + (i + 1));
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
