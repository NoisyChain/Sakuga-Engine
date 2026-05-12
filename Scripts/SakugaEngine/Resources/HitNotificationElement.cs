using Godot;
using System;

namespace SakugaEngine.Resources
{
	[GlobalClass]
    public partial class HitNotificationElement : Resource
    {
        [Export] public string NotificationName;
        [Export(PropertyHint.MultilineText)] public string Description;
        [Export] public int AnnouncerVoiceLine = -1;
    }
}
