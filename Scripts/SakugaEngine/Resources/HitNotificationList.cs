using Godot;
using System;

namespace SakugaEngine.Resources
{
	[GlobalClass]
	public partial class HitNotificationList : Resource
	{
		[Export] public HitNotificationElement[] Elements;
	}
}