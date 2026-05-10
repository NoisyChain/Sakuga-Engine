using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.UI
{
	public partial class MatchSettingsMenu : Control
	{
		[Export] private MainMenuManager Manager;
		[Export] private Button firstButton;
		[Export] private OptionButton TimeLimitOp;
		[Export] private OptionButton RoundsOp;
		[Export] private OptionButton BotDiffOp;
		[Export] private OptionButton PauseModeOp;

		public void GoTo()
        {
            Visible = true;
            firstButton.GrabFocus();
        }

		public void _OnGoButtonPressed()
		{
			Visible = false;
			int finalRoundTime = -1;
            switch (TimeLimitOp.Selected)
            {
                case 0:
                    finalRoundTime = 99;
                    break;
                case 1:
                    finalRoundTime = 90;
                    break;
                case 2:
                    finalRoundTime = 60;
                    break;
                case 3:
                    finalRoundTime = 30;
                    break;
                case 4:
                    finalRoundTime = 10;
                    break;
                case 5:
                    break;
            }
			Manager.matchSettings.TimeLimit = finalRoundTime;
			Manager.matchSettings.RoundsToWin = RoundsOp.Selected + 1;
			Manager.matchSettings.Difficulty = (BotDifficulty)BotDiffOp.Selected;
			Manager.inputMenu.GoTo();
		}

		public void _OnBackButtonPressed()
		{
			// Return to main menu
			Visible = false;
			Manager.mainMenu.GoTo();
		}
	}
}
