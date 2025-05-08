using Godot;

namespace PleaseResync
{
    public partial class ConnectionUI : Control
    {
        //private const uint MAX_PLAYERS = 2;
        //private const uint MAX_SPECTATORS = 8;
        public PleaseResyncManager manager;
        public ConnectionAddress[] playerConnections;
        public ConnectionAddress[] spectatorConnections;
        public LineEdit PlayerID;
        public LineEdit SpectatorCount;

        //[Export] public Button ConnectGameButton;
        //[Export] public Button LocalGameButton;
        //[Export] public Button CloseGameButton;
        public Control ConnectionMenuObject;
        public Control ConnectedMenuObject;
        public CheckButton IsSpectatorMode;

        public override void _Ready()
        {
            manager = GetTree().Root.GetNode<PleaseResyncManager>("Root/RollbackManager");
            ConnectionMenuObject = GetNode<Control>("Connection_Panel");
            ConnectedMenuObject = GetNode<Control>("Connected_Panel");
            PlayerID = ConnectionMenuObject.GetNode<LineEdit>("Player_Index");
            SpectatorCount = ConnectionMenuObject.GetNode<LineEdit>("Spectator_Count");
            IsSpectatorMode = ConnectionMenuObject.GetNode<CheckButton>("Spectator_Settings/Spectator_Toggle");

            playerConnections = new ConnectionAddress[manager.MaxPlayers];
            for (int i = 0; i < playerConnections.Length; i++)
            {
                string identity = "P" + (i + 1) + "_";

                playerConnections[i] = new ConnectionAddress(
                    ConnectionMenuObject.GetNode<LineEdit>(identity + "IP"),
                    ConnectionMenuObject.GetNode<LineEdit>(identity + "Port")
                );
            }

            spectatorConnections = new ConnectionAddress[manager.MaxSpectators];
            for (int i = 0; i < spectatorConnections.Length; i++)
            {
                string identity = "Spectator_Settings/S" + (i + 1) + "_";

                spectatorConnections[i] = new ConnectionAddress(
                    ConnectionMenuObject.GetNode<LineEdit>(identity + "IP"),
                    ConnectionMenuObject.GetNode<LineEdit>(identity + "Port")
                );
            }
        }
        
        private void OnButton_OnlinePlayPressed()
        {
            StartOnlineGame();
        }

        private void OnButton_OfflinePlayPressed()
        {
            StartLocalGame();
        }

        private void OnButton_ReplayPressed()
        {
            StartReplay();
        }

        private void OnButton_ClosePressed()
        {
            CloseGame();
        }

        private void StartOnlineGame()
        {
            uint finalSpectatorCount = SpectatorCount.Text.Trim().Length > 0 ? uint.Parse(SpectatorCount.Text) : 0;
            uint finalPlayerID = PlayerID.Text.Trim().Length > 0 ? uint.Parse(PlayerID.Text) : 0;
            bool spectate = IsSpectatorMode.ButtonPressed;
            manager.CreatePlayerConnections(CreateAddressesList(manager.MaxPlayers), CreatePortsList(manager.MaxPlayers));
            manager.CreateSpectatorConnections(CreateAddressesList(finalSpectatorCount, true), CreatePortsList(finalSpectatorCount, true));
            manager.OnlineGame(spectate, manager.MaxPlayers, finalSpectatorCount, finalPlayerID);
            ConnectionMenuObject.Visible = false;
            ConnectedMenuObject.Visible = true;
            GD.Print("Online game started.");
        }

        private void StartLocalGame()
        {
            manager.LocalGame(manager.MaxPlayers);
            ConnectionMenuObject.Visible = false;
            ConnectedMenuObject.Visible = true;
            GD.Print("Local game started.");
        }

        private void StartReplay()
        {
            manager.ReplayMode(manager.MaxPlayers);
            ConnectionMenuObject.Visible = false;
            ConnectedMenuObject.Visible = true;
            GD.Print("Executing replay...");
        }

        private void CloseGame()
        {
            manager.CloseGame();
            ConnectionMenuObject.Visible = true;
            ConnectedMenuObject.Visible = false;
            GD.Print("Game aborted.");
        }

        private string[] CreateAddressesList(uint length, bool isSpectator = false)
        {
            string[] temp = new string[length];
            for (int i = 0; i < temp.Length; ++i)
            {
                string address = "";
                if (isSpectator)
                    spectatorConnections[i].IPField.Text.Trim();
                else
                    playerConnections[i].IPField.Text.Trim();
                
                temp[i] = address.Length > 0 ? address : "127.0.0.1";
            }

            return temp;
        }

        private ushort[] CreatePortsList(uint length, bool isSpectator = false)
        {
            ushort[] temp = new ushort[length];
            for (int i = 0; i < temp.Length; ++i)
            {
                string port = "";
                if (isSpectator)
                    spectatorConnections[i].PortField.Text.Trim();
                else
                    playerConnections[i].PortField.Text.Trim();
                temp[i] = port.Length > 0 ? ushort.Parse(port) : (ushort)0;
            }

            return temp;
        }
    }

    [System.Serializable]
    public class ConnectionAddress
    {
        public LineEdit IPField;
        public LineEdit PortField;

        public ConnectionAddress(LineEdit _IP, LineEdit _Port)
        {
            IPField = _IP;
            PortField = _Port;
        }
    }
}

