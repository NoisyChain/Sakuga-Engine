using Godot;

namespace PleaseResync
{
    public partial class ConnectionUI : Control
    {
        private const uint MAX_CONNECTIONS = 2;
        public PleaseResyncManager manager;
        public ConnectionAddress[] connectionAdresses;
        public LineEdit PlayerID;
        //[Export] public Button ConnectGameButton;
        //[Export] public Button LocalGameButton;
        //[Export] public Button CloseGameButton;
        public Control ConnectionMenuObject;
        public Control ConnectedMenuObject;

        public override void _Ready()
        {
            manager = GetTree().Root.GetNode<PleaseResyncManager>("Root/RollbackManager");
            ConnectionMenuObject = GetNode<Control>("Connection_Panel");
            ConnectedMenuObject = GetNode<Control>("Connected_Panel");
            PlayerID = ConnectionMenuObject.GetNode<LineEdit>("Player_Index");

            connectionAdresses = new ConnectionAddress[MAX_CONNECTIONS];
            connectionAdresses[0] = new ConnectionAddress(
                ConnectionMenuObject.GetNode<LineEdit>("P1_IP"),
                ConnectionMenuObject.GetNode<LineEdit>("P1_Port")
            );
            connectionAdresses[1] = new ConnectionAddress(
                ConnectionMenuObject.GetNode<LineEdit>("P2_IP"),
                ConnectionMenuObject.GetNode<LineEdit>("P2_Port")
            );
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
            uint finalPlayerID = PlayerID.Text.Trim().Length > 0 ? uint.Parse(PlayerID.Text) : 0;
            manager.CreateConnections(CreateAddressesList(), CreatePortsList());
            manager.OnlineGame(MAX_CONNECTIONS, finalPlayerID);
            ConnectionMenuObject.Visible = false;
            ConnectedMenuObject.Visible = true;
            GD.Print("Online game started.");
        }

        private void StartLocalGame()
        {
            manager.LocalGame(MAX_CONNECTIONS);
            ConnectionMenuObject.Visible = false;
            ConnectedMenuObject.Visible = true;
            GD.Print("Local game started.");
        }

        private void StartReplay()
        {
            manager.ReplayMode(MAX_CONNECTIONS);
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

        private string[] CreateAddressesList()
        {
            string[] temp = new string[MAX_CONNECTIONS];
            for (int i = 0; i < temp.Length; ++i)
            {
                string address = connectionAdresses[i].IPField.Text.Trim();
                temp[i] = address.Length > 0 ? address : "";
            }

            return temp;
        }

        private ushort[] CreatePortsList()
        {
            ushort[] temp = new ushort[MAX_CONNECTIONS];
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

