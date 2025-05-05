using Godot;

namespace PleaseResync
{
    public partial class ConnectionUI : Control
    {
        private const uint MAX_PLAYERS = 2;
        private const uint MAX_SPECTATORS = 2;
        public PleaseResyncManager manager;
        public ConnectionAddress[] connectionAdresses;
        public LineEdit PlayerID;
        public LineEdit SpectatorCount;
        public LineEdit SpectatorID;
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
            IsSpectatorMode = ConnectionMenuObject.GetNode<CheckButton>("Spectator_Settings/Spectator_Toggle");
            SpectatorCount = ConnectionMenuObject.GetNode<LineEdit>("Spectator_Settings/Spectator_Count");
            SpectatorID = ConnectionMenuObject.GetNode<LineEdit>("Spectator_Settings/Spectator_Index");

            connectionAdresses = new ConnectionAddress[MAX_PLAYERS/* + MAX_SPECTATORS*/];
            for (int i = 0; i < connectionAdresses.Length; i++)
            {
                string identity = "P" + (i + 1) + "_";

                connectionAdresses[i] = new ConnectionAddress(
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
            uint totalPlayers = MAX_PLAYERS + (uint)Mathf.Min(MAX_SPECTATORS, finalSpectatorCount);
            uint finalPlayerID = PlayerID.Text.Trim().Length > 0 ? uint.Parse(PlayerID.Text) : 0;
            uint finalSpectatorID = SpectatorID.Text.Trim().Length > 0 ? uint.Parse(SpectatorID.Text) : 0;
            manager.CreateConnections(CreateAddressesList(totalPlayers), CreatePortsList(totalPlayers));
            if (IsSpectatorMode.ButtonPressed)
                manager.Spectate(MAX_PLAYERS, finalSpectatorCount, MAX_PLAYERS + finalSpectatorID);
            else
                manager.OnlineGame(MAX_PLAYERS, finalSpectatorCount, finalPlayerID);
            ConnectionMenuObject.Visible = false;
            ConnectedMenuObject.Visible = true;
            GD.Print("Online game started.");
        }

        private void StartLocalGame()
        {
            manager.LocalGame(MAX_PLAYERS);
            ConnectionMenuObject.Visible = false;
            ConnectedMenuObject.Visible = true;
            GD.Print("Local game started.");
        }

        private void StartReplay()
        {
            manager.ReplayMode(MAX_PLAYERS);
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

        private string[] CreateAddressesList(uint length)
        {
            string[] temp = new string[length];
            for (int i = 0; i < temp.Length; ++i)
            {
                string address = connectionAdresses[i].IPField.Text.Trim();
                temp[i] = address.Length > 0 ? address : "";
            }

            return temp;
        }

        private ushort[] CreatePortsList(uint length)
        {
            ushort[] temp = new ushort[length];
            for (int i = 0; i < temp.Length; ++i)
            {
                string port = connectionAdresses[0].PortField.Text.Trim();
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

