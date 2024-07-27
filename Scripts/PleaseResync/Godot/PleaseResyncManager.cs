using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;

namespace PleaseResync
{
    public partial class PleaseResyncManager : Node
    {
        [Export] public bool SyncTest;
        private Label SimulationInfo;
        private Label RollbackInfo;
        private Label PingInfo;

        private bool Started;
        private bool Replay;
        
        [Export] protected ushort FrameDelay = 2;
        [Export] protected ushort SimulatedFrameDelay = 0;
        [Export] protected uint MaxPlayers = 2;
        [Export] protected uint DeviceCount = 2;
        [Export] protected uint InputSize = 1;
        private uint DEVICE_ID;

        private string[] Adresses = {"127.0.0.1", "127.0.0.1", "127.0.0.1", "127.0.0.1"};
        private ushort[] Ports = {7001, 7002, 7003, 7004};

        public IGameState sessionState;

        LiteNetLibSessionAdapter adapter;
        Session session;
        byte[] LastInput;
        List<SessionAction> sessionActions;
        string InputDebug;
        string SimulationText;
        List<ReplayInputs> RecordedInputs = new List<ReplayInputs>();

        Thread PingThread;

        Ping p = new Ping();
        PingReply r;

        private void StartPinging(string pingIP)
        {
            PingThread = new Thread(() => Ping(pingIP));
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
            RecordedInputs.Add(new ReplayInputs());

            if (SimulationInfo != null) SimulationInfo.Text = "";
            if (RollbackInfo != null) RollbackInfo.Text = "";
            if (PingInfo != null) PingInfo.Text = "";
        }
        /*public override void _Process(double delta)
        {
            if (!Started) return;
            if (!OfflineMode)
            {
                if (RollbackInfo != null) RollbackInfo.Text = "RBF: " + session.RollbackFrames();
                if (PingInfo != null) PingInfo.Text = ShowPingInfo();
            }
            if (SimulationInfo != null) SimulationInfo.Text = SimulationText;
        }*/

        public override void _PhysicsProcess(double delta)
        {
            if (!Started) return;

            if (SimulationInfo != null) SimulationInfo.Text = NotificationText();

            if (!session.IsOffline())
            {
                session.Poll();

                if (!session.IsRunning()) return;
                
                if (RollbackInfo != null) RollbackInfo.Text = "RBF: " + session.RollbackFrames();
                if (PingInfo != null) PingInfo.Text = ShowPingInfo();
            }

            GameLoop();
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

            DEVICE_ID = ID;
            //MAX_PLAYERS = playerCount;
            //DEVICE_COUNT = playerCount;

            sessionState = state;

            sessionState.Setup();
            
            adapter = new LiteNetLibSessionAdapter(Adresses[DEVICE_ID], Ports[DEVICE_ID]);

            session = new Peer2PeerSession(InputSize, DeviceCount, MaxPlayers, false, adapter);

            LastInput = new byte[InputSize];

            session.SetLocalDevice(DEVICE_ID, 1, FrameDelay);
            
            for (uint i = 0; i < DeviceCount; i++)
            {
                if (i != DEVICE_ID)
                {
                    session.AddRemoteDevice(i, 1, LiteNetLibSessionAdapter.CreateRemoteConfig(Adresses[i], Ports[i]));
                    tempIP = Adresses[i];
                    
                    GD.Print($"Device {i} created");
                }
            }
            
            Replay = false;
            Started = true;
            StartPinging(tempIP);
        }

        protected void StartOfflineGame(IGameState state, uint playerCount)
        {
            sessionState = state;
            sessionState.Setup();

            session = new Peer2PeerSession(InputSize, 1, MaxPlayers, true, null);
            LastInput = new byte[(int)(MaxPlayers * InputSize)];
            session.SetLocalDevice(DEVICE_ID, MaxPlayers, SimulatedFrameDelay);

            if (RollbackInfo != null) RollbackInfo.Text = "";
            if (PingInfo != null) PingInfo.Text = "";

            Replay = false;
            Started = true;
        }

        protected void StartReplay(IGameState state, uint playerCount)
        {
            sessionState = state;
            sessionState.Setup();

            session = new Peer2PeerSession(InputSize, 1, MaxPlayers, true, null);
            LastInput = new byte[(int)(MaxPlayers * InputSize)];
            session.SetLocalDevice(DEVICE_ID, MaxPlayers, SimulatedFrameDelay);

            if (RollbackInfo != null) RollbackInfo.Text = "";
            if (PingInfo != null) PingInfo.Text = "";

            Replay = true;
            Started = true;
        }

        private void GameLoop()
        {
            if (Replay)
                LastInput = ReadInputs(session.Frame());
            else
                for (int i = 0; i < LastInput.Length / InputSize; i++)
                {
                    byte[] inputs = SyncTest ? GetRandomInput() : sessionState.GetLocalInput(i, (int)InputSize);
                    Array.Copy(inputs, 0, LastInput, i * InputSize, inputs.Length);
                }

            sessionActions = session.AdvanceFrame(LastInput);
            
            foreach (var action in sessionActions)
            {
                switch (action)
                {
                    case SessionAdvanceFrameAction AFAction:
                        InputDebug = InputConstructor(AFAction.Inputs);
                        sessionState.GameLoop(AFAction.Inputs);
                        if (!Replay) RecordInput(AFAction.Frame, AFAction.Inputs);
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
                        byte[] state = writerStream.ToArray();
                        
                        SGAction.Save(state, Platform.GetChecksum(state));
                        break;
                }
            }

            string FrameCounter = session.IsOffline() ? $"{session.Frame()}" : $"{session.Frame()} ({session.FrameAdvantage()})";

            SimulationText = FrameCounter + $" || ( {InputDebug} )";

            if (Replay && session.Frame() >= RecordedInputs.Count)
                CloseGame();
        }

        private string NotificationText()
        {
            switch (session.State())
            {
                default:
                    return "";
                case 0:
                    return "Syncing...";
                case 1:
                    return SimulationText;
                case 2:
                    return "Connection lost.";
                case 3:
                    return "Fatal desync detected.";
            }
        }

        public void CloseGame()
        {
            sessionState = null;
            if (adapter != null) adapter.Close();
            Replay = false;
            Started = false;
            SimulationInfo.Text = "Disconnected";
        }

        private string InputConstructor(byte[] PlayerInputs)
        {
            string finalString = " ";

            for (int i = 0; i < PlayerInputs.Length; i++)
            {
                finalString += PlayerInputs[i].ToString() + " ";
                if (i < PlayerInputs.Length - 1) finalString += ":: ";
            }

            return finalString;
        }

        private void RecordInput(int frame, byte[] inputs)
        {
            if (frame >= RecordedInputs.Count)
                RecordedInputs.Add(new ReplayInputs());
            
            RecordedInputs[frame] = new ReplayInputs(inputs);
        }

        private byte[] ReadInputs(int frame)
        {
            return RecordedInputs[frame].inputs;
        }

        private byte[] GetRandomInput()
        {
            byte[] cnv = new byte[InputSize];
            for (int i = 0; i < cnv.Length; i++)
            {
                cnv[i] = (byte)Platform.GetRandomUnsignedShort();
            }

            return cnv;
        }

        public virtual void OnlineGame(uint maxPlayers, uint ID){}
        public virtual void LocalGame(uint maxPlayers){}
        public virtual void ReplayMode(uint maxPlayers) {}
    }

    public struct ReplayInputs
    {
        public byte[] inputs;

        public ReplayInputs()
        {
            inputs = new byte[0];
        }

        public ReplayInputs(byte[] i)
        {
            inputs = i;
        }
    };
}
