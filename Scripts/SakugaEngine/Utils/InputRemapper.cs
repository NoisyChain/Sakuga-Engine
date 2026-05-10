using Godot;
using Godot.Collections;

namespace SakugaEngine.Utils
{
	public partial class InputRemapper : Control
	{
		const string KeymapDir = "user://keymaps.dat";
		const string DefaultDir = "user://keymaps_default.dat";
		private Dictionary<StringName, Array<InputEvent>> Keymaps = new Dictionary<StringName, Array<InputEvent>>();

		[Export] private Control[] InputMappingChildren;
		[Export] private Control P1Pivot;
		[Export] private Control P2Pivot;
		[Export] private Control P1CurrentActiveInputMapping;
		[Export] private Control P2CurrentActiveInputMapping;

		public bool IsP1Remapping => P1CurrentActiveInputMapping != null && P1CurrentActiveInputMapping.Visible;
		public bool IsP2Remapping => P2CurrentActiveInputMapping != null && P2CurrentActiveInputMapping.Visible;
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			foreach(var action in InputMap.GetActions())
			{
				if (InputMap.ActionGetEvents(action).Count != 0)
				{
					Keymaps.Add(action, InputMap.ActionGetEvents(action));
				}
			}

			CreateDefaultKeymaps();
			LoadKeymap();
		}

		public void ToggleInputMapperP1(int index)
		{
			P1CurrentActiveInputMapping = InputMappingChildren[index];
			P1CurrentActiveInputMapping.Position = P1Pivot.Position;
			P1CurrentActiveInputMapping.Visible = !P1CurrentActiveInputMapping.Visible;
			P1CurrentActiveInputMapping._Ready();
		}

		public void ToggleInputMapperP2(int index)
		{
			P2CurrentActiveInputMapping = InputMappingChildren[index];
			P2CurrentActiveInputMapping.Position = P2Pivot.Position;
			P2CurrentActiveInputMapping.Visible = !P2CurrentActiveInputMapping.Visible;
			P2CurrentActiveInputMapping._Ready();
		}

		public void CreateDefaultKeymaps()
		{
			if (FileAccess.FileExists(DefaultDir)) return;

			var file = FileAccess.Open(DefaultDir, FileAccess.ModeFlags.Write);
			file.StoreVar(Keymaps, true);
			file.Close();
		}

		public void ReturnToDefaultKeymaps(Array<StringName> actions)
		{
			var file = FileAccess.Open(DefaultDir, FileAccess.ModeFlags.Read);
			var tempKeymap = (Dictionary<StringName, Array<InputEvent>>)file.GetVar(true);
			file.Close();

			foreach(var action in actions)
			{
				if (tempKeymap.TryGetValue(action, out Array<InputEvent> ev))
				{
					Remap(action, ev);
					InputMap.ActionEraseEvents(action);
					foreach (InputEvent newEv in ev)
					{
						InputMap.ActionAddEvent(action, newEv);
					}
				}
			}

			SaveKeymap();
		}

		public void Remap(StringName action, Array<InputEvent> ev)
		{
			if (!Keymaps.ContainsKey(action))
			{
				Keymaps.Add(action, ev);
				return;
			}

			Keymaps[action] = ev;
		}

		public void SaveKeymap()
		{
			var file = FileAccess.Open(KeymapDir, FileAccess.ModeFlags.Write);
			file.StoreVar(Keymaps, true);
			file.Close();
		}

		public void LoadKeymap()
		{
			if (!FileAccess.FileExists(KeymapDir))
			{
				SaveKeymap();
				return;
			}
			var file = FileAccess.Open(KeymapDir, FileAccess.ModeFlags.Read);
			var tempKeymap = (Dictionary<StringName, Array<InputEvent>>)file.GetVar(true);
			file.Close();

			foreach(var action in InputMap.GetActions())
			{
				if (tempKeymap.TryGetValue(action, out Array<InputEvent> ev))
				{
					Remap(action, ev);
					InputMap.ActionEraseEvents(action);
					foreach (InputEvent newEv in ev)
					{
						InputMap.ActionAddEvent(action, newEv);
					}
				}
			}
		}
	}
}
