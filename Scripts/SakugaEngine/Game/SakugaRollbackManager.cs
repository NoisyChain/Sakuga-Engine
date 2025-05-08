using Godot;
using System;
using PleaseResync;

namespace SakugaEngine.Game
{
    public partial class SakugaRollbackManager : PleaseResyncManager
    {
        [Export] private bool AutoStart;

        [Export] private GameManager GameManager;

        public override void _Ready()
        {
            base._Ready();
            GameManager.InputSize = InputSize;
            
            if (AutoStart)
            {
                GameManager.player1Character = Global.Match.Player1.selectedCharacter;
                GameManager.player2Character = Global.Match.Player2.selectedCharacter;
                GameManager.selectedStage = Global.Match.selectedStage;
                GameManager.selectedBGM = Global.Match.selectedBGM;
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
