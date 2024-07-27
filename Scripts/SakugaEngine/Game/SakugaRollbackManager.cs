using Godot;
using System;
using PleaseResync;

namespace SakugaEngine.Game
{
    public partial class SakugaRollbackManager : PleaseResyncManager
    {
        private const uint MAX_PLAYERS = 2;
        private const uint MAX_SPECTETORS = 10;

        [Export] private GameManager GameManager;

        public override void _Ready()
        {
            base._Ready();
            GameManager.InputSize = InputSize;
        }

        public override void OnlineGame(uint maxPlayers, uint ID)
        {
            StartOnlineGame(GameManager, maxPlayers, ID);
        }

        public override void LocalGame(uint maxPlayers)
        {
            StartOfflineGame(GameManager, maxPlayers);
        }

        public override void ReplayMode(uint maxPlayers)
        {
            StartReplay(GameManager, maxPlayers);
        }
    }
}
