using Godot;
using SakugaEngine.Global;
using System;

namespace SakugaEngine.UI
{
	public partial class HitNotificationController : Control
	{
		[Export] private HitNotificationWidget[] Widgets;
		[Export] private FrameTimer Timer;

		public void View(HitNotificationPiece[] elements)
		{
			for (int i = 0; i < Widgets.Length; i++)
			{
				Widgets[i].View(elements[i]);
			}
		}
	}
}
