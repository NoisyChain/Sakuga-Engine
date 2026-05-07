using Godot;
using SakugaEngine.Game;

namespace SakugaEngine.UI
{
	public partial class PauseMenu : Control
	{
		[Export] private Button firstButton;
		public void CallPauseMenu(bool paused)
        {
			Visible = paused;
			if (paused)
            	firstButton.GrabFocus();
		}
		private void _on_button_resume_pressed()
		{
			if (!Visible) return;
			GameManager.Instance.PauseControl(false);
		}

		private void _on_button_restart_pressed()
		{
			if (!Visible) return;
			LoadingScreenManager.Instance.LoadScene("res://Scenes/TestScene.tscn");
			Visible = false;
		}

		private void _on_button_char_select_pressed()
		{
			if (!Visible) return;
			LoadingScreenManager.Instance.LoadScene("res://Scenes/SelectScreen.tscn");
			Visible = false;
		}

		private void _on_button_main_menu_pressed()
		{
			if (!Visible) return;
			LoadingScreenManager.Instance.LoadScene("res://Scenes/MainMenu.tscn");
			Visible = false;
		}
	}
}
