using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class GameModeSettings : Resource
    {
        [Export] public string ModeName;
        [Export] public bool AutoStart;
        [Export] public Global.NetcodeMode NetcodeMode;
        [Export] public bool UseLAN;
        [Export] public bool CanKO;
        [Export] public bool ShowMatchIntro; // doesn't exist
        [Export] public bool ShowMatchCards; // doesn't exist
        [Export] public bool ShowTrainingInfo;
        [Export] public bool ShowOnlineInfo;
        //[Export] public bool ShowTimeline; // For replay mode (doesn't exist)
        //[Export] public bool ShowComboList; // For combo challenge mode (doesn't exist)
        [Export] public bool AllowShowHitboxes;
        [Export] public bool AllowShowInputs;
        [Export] public bool AllowShowDebugElements;
        [Export] public bool AllowUseDebugCommands;
        [Export] public bool NoAI;
        [Export] public string ReturnToScene;
    }
}
