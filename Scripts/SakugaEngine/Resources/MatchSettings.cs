using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class MatchSettings : Resource
    {
        [ExportCategory("Main Settings")]
        [Export] public GameModeSettings[] GameModes;
        [Export] public int SelectedMode;
        [Export] public int TimeLimit = -1;
        [Export] public int RoundsToWin = 2;
        [Export] public Global.BotDifficulty Difficulty = Global.BotDifficulty.MEDIUM;
        [ExportCategory("Player 1")]
        [Export] public PackedScene P1SelectedCharacter;
        [Export] public int P1SelectedDevice = -1; // -1 = CPU player
        [ExportCategory("Player 2")]
        [Export] public PackedScene P2SelectedCharacter;
        [Export] public int P2SelectedDevice = -1; // -1 = CPU player
        [ExportCategory("Stage and Music")]
        [Export] public PackedScene SelectedStage;
        [Export] public AudioStream SelectedBGM;
        [ExportCategory("Online Settings")]
        [Export] public string OnlineP1Name;
        [Export] public Texture2D OnlineP1PlatformIcon;
        [Export] public string OnlineP2Name;
        [Export] public Texture2D OnlineP2PlatformIcon;
        [Export] public bool IsSpectator;
        [Export] public int PlayerID;
        [Export] public int SpectatorCount;

        public GameModeSettings SelectedModeSettings => GameModes[SelectedMode];
    }
}
