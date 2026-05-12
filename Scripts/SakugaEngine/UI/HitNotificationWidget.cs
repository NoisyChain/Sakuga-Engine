using Godot;
using SakugaEngine.Global;
using System;

namespace SakugaEngine.UI
{
	public partial class HitNotificationWidget : Control
	{
		[Export] private Label NotifLabel;

		public void View(HitNotificationPiece element)
		{
			Visible = element.Active;
			NotifLabel.Text = element.NotifName;
		}
	}
}
