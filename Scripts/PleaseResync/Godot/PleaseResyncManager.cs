using Godot;
using SakugaEngine.Utils;
using System.IO;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using SakugaEngine.Game;

namespace PleaseResync
{
    public partial class PleaseResyncManager : Node
    {
        private Label SimulationInfo;
        private Label RollbackInfo;
        private Label PingInfo;
        //private InputHistory P1InputHistory;
        //private InputHistory P2InputHistory;

        private bool OfflineMode;
        private bool Started;
        private bool Replay;
        private const uint INPUT_SIZE = 1;
        private const ushort FRAME_DELAY = 2;
        private uint MAX_PLAYERS = 2;
        private uint DEVICE_COUNT = 2;
        private uint DEVICE_ID;

        private string[] Adresses = {"127.0.0.1", "127.0.0.1", "127.0.0.1", "127.0.0.1"};
        private ushort[] Ports = {7001, 7002, 7003, 7004};

        [Export] private GameManager GameManager;
        public IGameState sessionState;

        UdpSessionAdapter adapter;
        Session session;
        PlayerInputs[] LastInput;
        List<SessionAction> sessionActions;
        string InputDebug;
        List<PlayerInputs[]> RecordedInputs = new List<PlayerInputs[]>();

        System.Threading.Thread PingThread;
        Ping p = new Ping();
        PingReply r;

        private void StartPinging(string pingIP)
        {
            PingThread = new System.Threading.Thread(() => Ping(pingIP));
            PingThread.IsBackground = true;
            PingThread.Start();
        }

        private void Ping(string PingIP)
        {
            while(Started)
                r = p.Send(PingIP);
        }

        private string ShowPingInfo()
        {
            if (r == null) return "";
            if (r.Status != IPStatus.Success) return "";
            return $"Ping: {r.RoundtripTime} ms";
        }

        public override void _Ready()
        {
            SimulationInfo = GetTree().Root.GetNode<Label>("Root/CanvasLayer/PleaseResync_UI/Connection_Status/Simulation_Info");
            RollbackInfo = GetTree().Root.GetNode<Label>("Root/CanvasLayer/PleaseResync_UI/Connection_Status/Rollback_Info");
            PingInfo = GetTree().Root.GetNode<Label>("Root/CanvasLayer/PleaseResync_UI/Connection_Status/Ping_Info");
            //P1InputHistory = GetTree().Root.GetNode<InputHistory>("Root/CanvasLayer/GameHUD_Foreground/InputHistory/P1InputHistory");
            //P2InputHistory = GetTree().Root.GetNode<InputHistory>("Root/CanvasLayer/GameHUD_Foreground/InputHistory/P2InputHistory");
            RecordedInputs.Add(new PlayerInputs[0]);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!Started) return;

            if (OfflineMode)
            {
                if (Replay)
                    ReplayLoop();
                else
                    OfflineGameLoop();
                
                /*if (sessionState != null)
                {
                    P1InputHistory.SetHistoryList(LastInput[0], sessionState.StateFrame());
                    P2InputHistory.SetHistoryList(LastInput[1], sessionState.StateFrame());
                }*/
            }
            else
            {
                session.Poll();

                if (!session.IsRunning())
                {
                    if (SimulationInfo != null) SimulationInfo.Text = "Syncing...";
                    return;
                }
                
                OnlineGameLoop();
                if (RollbackInfo != null) RollbackInfo.Text = "RBF: " + session.RollbackFrames();
                if (PingInfo != null) PingInfo.Text = ShowPingInfo();
            }
        }

        void OnDestroy()
        {
            CloseGame();
        }

        public void CreateConnections(string[] IPAdresses, ushort[] ports)
        {
            for (uint i = 0; i < IPAdresses.Length; i++)
            {
                if (IPAdresses[i] != "") Adresses[i] = IPAdresses[i];
                if (ports[i] > 0) Ports[i] = ports[i];
            }
        }

        protected void StartOnlineGame(IGameState state, uint playerCount, uint ID)
        {
            string tempIP = "";
            OfflineMode = false;

            DEVICE_ID = ID;
            MAX_PLAYERS = playerCount;
            DEVICE_COUNT = playerCount;

            sessionState = state;

            sessionState.Setup();
            
            adapter = new UdpSessionAdapter(Adresses[DEVICE_ID], Ports[DEVICE_ID]);

            session = new Peer2PeerSession(INPUT_SIZE, DEVICE_COUNT, MAX_PLAYERS, adapter);

            LastInput = new PlayerInputs[INPUT_SIZE];

            session.SetLocalDevice(DEVICE_ID, 1, FRAME_DELAY);
            
            for (uint i = 0; i < DEVICE_COUNT; i++)
            {
                if (i != DEVICE_ID)
                {
                    session.AddRemoteDevice(i, 1, UdpSessionAdapter.CreateRemoteConfig(Adresses[i], Ports[i]));
                    tempIP = Adresses[i];
                    
                    GD.Print($"Device {i} created");
                }
            }
            //session.Poll();

            Replay = false;
            Started = true;
            StartPinging(tempIP);
        }

        protected void StartOfflineGame(IGameState state, uint playerCount)
        {
            OfflineMode = true;

            sessionState = state;

            sessionState.Setup();

            //session = new LocalSession(INPUT_SIZE, 1, MAX_PLAYERS);

            //session.SetLocalDevice(0, 2, 0);

            LastInput = new PlayerInputs[(int)playerCount];
            if (RollbackInfo != null) RollbackInfo.Text = "";
            if (PingInfo != null) PingInfo.Text = "";

            Replay = false;
            Started = true;
        }

        protected void StartReplay(IGameState state, uint playerCount)
        {
            OfflineMode = true;
            sessionState = state;

            LastInput = new PlayerInputs[(int)playerCount];
            if (RollbackInfo != null) RollbackInfo.Text = "";
            if (PingInfo != null) PingInfo.Text = "";

            Replay = true;
            Started = true;
        }

        private void OnlineGameLoop()
        {
            for (int i = 0; i < LastInput.Length; i++)
                LastInput[i] = sessionState.GetLocalInput(i);

            sessionActions = session.AdvanceFrame(LastInput);
            
            foreach (var action in sessionActions)
            {
                switch (action)
                {
                    case SessionAdvanceFrameAction AFAction:
                        InputDebug = InputConstructor(AFAction.Inputs);
                        sessionState.GameLoop(AFAction.Inputs);
                        //RecordInput(session.Frame(), AFAction.Inputs);
                        break;
                    case SessionLoadGameAction LGAction:
                        MemoryStream readerStream = new MemoryStream(LGAction.Load());
                        BinaryReader reader = new BinaryReader(readerStream);
                        sessionState.Deserialize(reader);
                        break;
                    case SessionSaveGameAction SGAction:
                        MemoryStream writerStream = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(writerStream);
                        sessionState.Serialize(writer);
                        SGAction.Save(writerStream.ToArray(), (uint)Fletcher.FletcherChecksum(writerStream.ToArray(), 32));
                        break;
                }
            }

            if (SimulationInfo != null) 
                SimulationInfo.Text = $"{sessionState.StateFrame()} ({session.FrameAdvantage()}) || ( {InputDebug} )";
        }

        private void OfflineGameLoop()
        {
            for (int i = 0; i < LastInput.Length; i++)
                LastInput[i] = sessionState.GetLocalInput(i);

            sessionState.GameLoop(LastInput);
            InputDebug = InputConstructor(LastInput);

            RecordInput(sessionState.StateFrame(), LastInput);

            if (SimulationInfo != null)
                SimulationInfo.Text = $"{sessionState.StateFrame()} || ( {InputDebug} )";
        }

        private void ReplayLoop()
        {
            LastInput = ReadInputs(sessionState.StateFrame());

            sessionState.GameLoop(LastInput);
            InputDebug = InputConstructor(LastInput);

            if (SimulationInfo != null)
                SimulationInfo.Text = $"{sessionState.StateFrame()} || ( {InputDebug} )";
            
            if (sessionState.StateFrame() >= RecordedInputs.Count)
                CloseGame();
        }

        public void CloseGame()
        {
            //P1InputHistory.Clear();
            //P2InputHistory.Clear();

            sessionState = null;
            if (adapter != null) adapter.Close();
            Replay = false;
            Started = false;
        }

        private string InputConstructor(PlayerInputs[] PlayerInputs)
        {
            string finalString = " ";

            for (int i = 0; i < PlayerInputs.Length; i++)
            {
                finalString += PlayerInputs[i].ToString() + " ";
                if (i < PlayerInputs.Length - 1) finalString += ":: ";
            }

            return finalString;
        }

        private void RecordInput(int frame, PlayerInputs[] inputs)
        {
            if (frame >= RecordedInputs.Count)
                RecordedInputs.Add(new PlayerInputs[0]);
            
            RecordedInputs[frame] = inputs;
        }

        private PlayerInputs[] ReadInputs(int frame)
        {
            return RecordedInputs[frame];
        }

        public virtual void OnlineGame(uint maxPlayers, uint ID)
        {
            StartOnlineGame(GameManager, maxPlayers, ID);
        }

        public virtual void LocalGame(uint maxPlayers)
        {
            StartOfflineGame(GameManager, maxPlayers);
        }

        public virtual void ReplayMode(uint maxPlayers) {}
    }
}
