using Godot;
using Godot.Collections;

public partial class InputRemapper : Node
{
	const string KeymapDir = "user://keymaps.dat";
	const string DefaultDir = "user://keymaps_default.dat";
	private Dictionary<StringName, Array<InputEvent>> Keymaps = new Dictionary<StringName, Array<InputEvent>>();
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
