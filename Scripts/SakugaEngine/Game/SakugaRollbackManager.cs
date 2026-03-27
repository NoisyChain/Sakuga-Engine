using Godot;
using PleaseResync;

namespace SakugaEngine.Game
{
    public partial class SakugaRollbackManager : PleaseResyncManager
    {
        [Export] private GameManager GameManager;

        public override void _Ready()
        {
            base._Ready();
            GameManager.InputSize = InputSize;
            UseLAN = GameManager.Match.SelectedModeSettings.UseLAN;

            GD.Print($"Starting {GameManager.Match.SelectedModeSettings.ModeName} mode.");

            if (GameManager.Match.SelectedModeSettings.AutoStart)
            {
                switch(GameManager.Match.SelectedModeSettings.NetcodeMode)
                {
                    case Global.NetcodeMode.LOCAL:
                        LocalGame(2);
                        break;
                    case Global.NetcodeMode.ONLINE:
                        OnlineGame(GameManager.Match.IsSpectator, 2, (uint)GameManager.Match.SpectatorCount, (uint)GameManager.Match.PlayerID);
                        break;
                    case Global.NetcodeMode.REPLAY:
                        ReplayMode(2);
                        break;
                }
            }
        }

        public override void OnlineGame(bool spectate, uint players, uint spectators, uint ID)
        {
            GameManager.SetBGM();
            StartOnlineGame(GameManager, spectate, players, spectators, ID);
        }

        public override void LocalGame(uint players)
        {
            GameManager.SetBGM();
            StartOfflineGame(GameManager, players);
        }

        public override void ReplayMode(uint players)
        {
            GameManager.SetBGM();
            StartReplay(GameManager, players);
        }
    }
}
