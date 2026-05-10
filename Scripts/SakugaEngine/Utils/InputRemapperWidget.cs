using Godot;
using Godot.Collections;

namespace SakugaEngine.Utils
{
	public partial class InputRemapperWidget : Control
	{
		[Export] private InputRemapper Remapper;
		[Export] private string Prefix;
		[Export(PropertyHint.Enum, "Keyboard,Gamepad")] private int DeviceType = 0;
		[Export] private int deviceID;
		[Export] private InputRemapperButton[] Buttons;
		[Export] private Color DefaultColor;
		[Export] private Color SelectedColor;
		[Export] private Color RemappingColor;
		[Export] private Timer TimeFrame;
		[Export] private InputRemapperWidget WaitFor;

		private bool remapping = false;

		private int Selected = 0;

		public bool IsRemapping => remapping;

		public override void _Ready()
		{
			for (int i = 0; i < Buttons.Length; i++)
			{
				UpdateText(i);
			}
			SetColors();
		}

        public override void _Process(double delta)
        {
			if (!Visible) return;
			
            base._Process(delta);
			if (remapping && TimeFrame.TimeLeft <= 0)
			{
				AudioManager.Instance.PlayMenuClip(2);
				remapping = false;
				UpdateText(Selected);
				SetColors();
			}
        }

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!Visible) return;
			if (WaitFor != null && WaitFor.IsRemapping) return;

			if (!remapping)
			{
				if (@event.IsActionPressed(Prefix + "up"))
				{
					Selected--;
					Selected = Mathf.Clamp(Selected, 0, Buttons.Length - 1);
					SetColors();
					AudioManager.Instance.PlayMenuClip(0);
				}
				if (@event.IsActionPressed(Prefix + "down"))
				{
					AudioManager.Instance.PlayMenuClip(0);
					Selected++;
					Selected = Mathf.Clamp(Selected, 0, Buttons.Length - 1);
					SetColors();
				}
				if (@event.IsActionPressed(Prefix + "accept"))
				{
					AudioManager.Instance.PlayMenuClip(1);
					//SetProcessUnhandledInput(true);
					Buttons[Selected].SetKey("Awaiting Input...");
					remapping = true;
					TimeFrame.Start();
					SetColors();
				}
				if (@event.IsActionPressed(Prefix + "return") || @event.IsActionPressed(Prefix + "options"))
				{
					AudioManager.Instance.PlayMenuClip(2);
					remapping = false;
					Visible = false;
				}
				if (@event.IsActionPressed(Prefix + "extra1"))
				{
					AudioManager.Instance.PlayMenuClip(2);
					ClearMapping();
				}
				if (@event.IsActionPressed(Prefix + "extra2"))
				{
					AudioManager.Instance.PlayMenuClip(2);
					Array<StringName> actions = new Array<StringName>();

					foreach(InputRemapperButton button in Buttons)
						actions.Add(Prefix + button.GetAction());

					Remapper.ReturnToDefaultKeymaps(actions);

					for (int i = 0; i < Buttons.Length; i++)
					{
						UpdateText(i);
					}
				}
				return;
			}

			switch (DeviceType)
			{
				case 0:
					RemapKeyboard(@event);
					break;
				case 1:
					RemapController(@event, deviceID);
					break;
			}
			
		}

		private void SetColors()
		{
			for (int i = 0; i < Buttons.Length; i++)
			{
				if (i == Selected)
					Buttons[i].SetColor(remapping ? RemappingColor : SelectedColor);
				else
					Buttons[i].SetColor(DefaultColor);
			}
		}

		private void RemapKeyboard(InputEvent @event)
		{
			if (@event is InputEventKey eventKey)
			{
				if (eventKey.Pressed)
				{
					InputMap.ActionEraseEvents(Prefix + Buttons[Selected].GetAction());
					InputMap.ActionAddEvent(Prefix + Buttons[Selected].GetAction(), eventKey);
					ReleaseFocus();
					UpdateText(Selected);
					Array<InputEvent> eventArray = new Array<InputEvent>();
					eventArray.Add(eventKey);
					Remapper.Remap(Prefix + Buttons[Selected].GetAction(), eventArray);
					Remapper.SaveKeymap();
					AudioManager.Instance.PlayMenuClip(3);
					remapping = false;
					SetColors();
				}
			}
		}

		private void RemapController(InputEvent @event, int deviceID)
		{
			if (@event.Device != deviceID) return;

			if (@event is InputEventJoypadButton eventPad)
			{
				if (eventPad.Pressed)
				{
					InputMap.ActionEraseEvents(Prefix + Buttons[Selected].GetAction());
					InputMap.ActionAddEvent(Prefix + Buttons[Selected].GetAction(), eventPad);
					ReleaseFocus();
					UpdateText(Selected);
					Array<InputEvent> eventArray = new Array<InputEvent>();
					eventArray.Add(eventPad);
					Remapper.Remap(Prefix + Buttons[Selected].GetAction(), eventArray);
					Remapper.SaveKeymap();
					AudioManager.Instance.PlayMenuClip(3);
					remapping = false;
					SetColors();
				}
			}
			else if (@event is InputEventJoypadMotion eventMotion)
			{
				if (Mathf.Abs(eventMotion.AxisValue) > 0.2f)
				{
					InputMap.ActionEraseEvents(Prefix + Buttons[Selected].GetAction());
					InputMap.ActionAddEvent(Prefix + Buttons[Selected].GetAction(), eventMotion);
					ReleaseFocus();
					UpdateText(Selected);
					Array<InputEvent> eventArray = new Array<InputEvent>();
					eventArray.Add(eventMotion);
					Remapper.Remap(Prefix + Buttons[Selected].GetAction(), eventArray);
					Remapper.SaveKeymap();
					AudioManager.Instance.PlayMenuClip(3);
					remapping = false;
					SetColors();
				}
			}
		}

		private void ClearMapping()
		{
			InputMap.ActionEraseEvents(Prefix + Buttons[Selected].GetAction());
			ReleaseFocus();
			Buttons[Selected].SetKey(" ");
			Remapper.Remap(Prefix + Buttons[Selected].GetAction(), null);
			Remapper.SaveKeymap();
		}

		private void UpdateText(int selected)
		{
			string actionStr = InputMap.ActionGetEvents(Prefix + Buttons[selected].GetAction())[0].AsText();
			if (DeviceType > 0) actionStr = GetSimplifiedGamepadButton(actionStr);

			Buttons[selected].SetKey(actionStr);
		}

		private string GetSimplifiedGamepadButton(string action)
		{
			if (action.Contains("Joypad Button 0")) return "Button A";
			else if (action.Contains("Joypad Button 1 ")) return "Button B";
			else if (action.Contains("Joypad Button 2 ")) return "Button X";
			else if (action.Contains("Joypad Button 3 ")) return "Button Y";
			else if (action.Contains("Joypad Button 4 ")) return "Button Back";
			else if (action.Contains("Joypad Button 5 ")) return "Button Home";
			else if (action.Contains("Joypad Button 6 ")) return "Button Start";
			else if (action.Contains("Joypad Button 7 ")) return "L.TB. Button";
			else if (action.Contains("Joypad Button 8 ")) return "R.TB. Button";
			else if (action.Contains("Joypad Button 9 ")) return "L. Shoulder";
			else if (action.Contains("Joypad Button 10")) return "R. Shoulder";
			else if (action.Contains("Joypad Button 11")) return "D-Pad Up";
			else if (action.Contains("Joypad Button 12")) return "D-Pad Down";
			else if (action.Contains("Joypad Button 13")) return "D-Pad Left";
			else if (action.Contains("Joypad Button 14")) return "D-Pad Right";
			else if (action.Contains("Joypad Motion on Axis 1") && action.Contains("with Value -0")) return "L.TB. Up";
			else if (action.Contains("Joypad Motion on Axis 1") && action.Contains("with Value 0")) return "L.TB. Down";
			else if (action.Contains("Joypad Motion on Axis 0") && action.Contains("with Value -0")) return "L.TB. Left";
			else if (action.Contains("Joypad Motion on Axis 0") && action.Contains("with Value 0")) return "L.TB. Right";
			else if (action.Contains("Joypad Motion on Axis 3") && action.Contains("with Value -0")) return "R.TB. Up";
			else if (action.Contains("Joypad Motion on Axis 3") && action.Contains("with Value 0")) return "R.TB. Down";
			else if (action.Contains("Joypad Motion on Axis 2") && action.Contains("with Value -0")) return "R.TB. Left";
			else if (action.Contains("Joypad Motion on Axis 2") && action.Contains("with Value 0")) return "R.TB. Right";
			else if (action.Contains("Joypad Motion on Axis 4")) return "Left Trigger";
			else if (action.Contains("Joypad Motion on Axis 5")) return "Right Trigger";

			return "";
		}
	}
}
