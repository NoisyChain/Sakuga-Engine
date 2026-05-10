using Godot;
using SakugaEngine.Resources;

namespace SakugaEngine.UI
{
	public partial class MainMenuManager : Node
	{
		[Export] public MatchSettings matchSettings;
		
		[Export] public MainMenu mainMenu;
		[Export] public InputSelectMenu inputMenu;
		[Export] public MatchSettingsMenu matchMenu;


		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
	}
}
