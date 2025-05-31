using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace PleaseResync
{
    public partial class PleaseResyncManager : Node
    {
        [Export] public bool SyncTest;
        private Label SimulationInfo;
        private Label RollbackInfo;
        private Label PingInfo;

        private bool Spectating;
        private bool Started;
        private bool Replay;

        [Export] protected bool UseSeparateThread = false;
        [Export] protected ushort FrameDelay = 2;
        [Export] protected ushort SimulatedFrameDelay = 0;
        [Export] protected ushort SpectatorDelay = 30;
        [Export] public uint MaxPlayers = 2;
        [Export] public uint MaxSpectators = 8;
        [Export] protected uint InputSize = 1;
        private uint DEVICE_ID;
        private uint PingId;

        string[] PlayerAddresses = { "127.0.0.1", "127.0.0.1" };
        string[] SpectatorAddresses = { "127.0.0.1", "127.0.0.1" };
        int[] PlayerPorts = { 7001, 7002 };
        int[] SpectatorPorts = { 8001, 8002 };

        public IGameState sessionState;

        LiteNetLibSessionAdapter adapter;
        Session session;
        byte[] LastInput;
        List<SessionAction> sessionActions;
        string InputDebug;
        string SimulationText;
        string PingText;
        string RollbackText;
        List<ReplayInputs> RecordedInputs = new List<ReplayInputs>();

        private Thread GameThread;
        private System.Threading.Mutex mutex = new System.Threading.Mutex();

        private string ShowPingInfo(uint id)
        {
            return session.AllDevices[id].GetRTT().ToString();
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

        public override void _PhysicsProcess(double delta)
        {
            if (!Started) return;

            if (!UseSeparateThread) GameLoop();
            mutex.WaitOne();
            sessionState.Render();
            mutex.ReleaseMutex();

            if (SimulationInfo != null) SimulationInfo.Text = NotificationText();
            if (RollbackInfo != null) RollbackInfo.Text = RollbackText;
            if (PingInfo != null) PingInfo.Text = PingText;
        }

        void OnDestroy()
        {
            CloseGame();
        }

        public void CreatePlayerConnections(string[] IPAdresses, ushort[] ports)
        {
            for (uint i = 0; i < IPAdresses.Length; i++)
            {
                if (IPAdresses[i] != "") PlayerAddresses[i] = IPAdresses[i];
                if (ports[i] > 0) PlayerPorts[i] = ports[i];
            }
        }
        
        public void CreateSpectatorConnections(string[] IPAdresses, ushort[] ports)
        {
            for (uint i = 0; i < IPAdresses.Length; i++)
            {
                if (IPAdresses[i] != "") SpectatorAddresses[i] = IPAdresses[i];
                if (ports[i] > 0) SpectatorPorts[i] = ports[i];
            }
        }

        protected void StartOnlineGame(IGameState state, bool spectate, uint playerCount, uint spectatorCount, uint ID)
        {
            DEVICE_ID = ID;
            sessionState = state;
            sessionState.Setup();
            LastInput = new byte[InputSize];

            if (!spectate)
            {
                adapter = new LiteNetLibSessionAdapter(PlayerAddresses[DEVICE_ID], (ushort)PlayerPorts[DEVICE_ID]);
                session = new Peer2PeerSession(InputSize, playerCount, MaxPlayers, false, adapter);
                session.SetLocalDevice(DEVICE_ID, 1, FrameDelay);

                for (uint i = 0; i < playerCount; i++)
                {
                    if (i != DEVICE_ID)
                    {
                        session.AddRemoteDevice(i, 1, LiteNetLibSessionAdapter.CreateRemoteConfig(PlayerAddresses[i], (ushort)PlayerPorts[i]));
                        PingId = i;

                        GD.Print($"Device {i} created");
                    }
                }
                // Add spectators
                if (DEVICE_ID < playerCount)
                {
                    for (uint i = 0; i < spectatorCount; i++)
                    {
                        if (i % playerCount != DEVICE_ID) continue;
                        session.AddSpectatorDevice(LiteNetLibSessionAdapter.CreateRemoteConfig(SpectatorAddresses[i], (ushort)SpectatorPorts[i]));
                    }
                }
            }
            else
            {
                // Let's spectate!
                adapter = new LiteNetLibSessionAdapter(SpectatorAddresses[DEVICE_ID], (ushort)SpectatorPorts[DEVICE_ID]);
                session = new SpectatorSession(InputSize, playerCount, adapter, SpectatorDelay);
                // Let's just make the first active player the broadcaster for now.
                // be sure to pass ALL the players as in player count
                uint targetDevice = DEVICE_ID % playerCount;
                session.AddRemoteDevice(targetDevice, playerCount, LiteNetLibSessionAdapter.CreateRemoteConfig(PlayerAddresses[targetDevice], (ushort)PlayerPorts[targetDevice]));
                GD.Print($"Spectator device id {DEVICE_ID} connected to player ID {targetDevice}");
            }

            Spectating = spectate;
            Replay = false;
            Started = true;

            StartGameThread();
        }

        protected void StartOfflineGame(IGameState state, uint playerCount)
        {
            sessionState = state;
            sessionState.Setup();
            session = new Peer2PeerSession(InputSize, 1, playerCount, true, null);
            LastInput = new byte[(int)(playerCount * InputSize)];
            session.SetLocalDevice(0, playerCount, SimulatedFrameDelay);

            if (RollbackInfo != null) RollbackInfo.Text = "";
            if (PingInfo != null) PingInfo.Text = "";

            Spectating = false;
            Replay = false;
            Started = true;

            StartGameThread();
        }

        protected void StartReplay(IGameState state, uint playerCount)
        {
            sessionState = state;
            sessionState.Setup();
            session = new Peer2PeerSession(InputSize, 1, playerCount, true, null);
            LastInput = new byte[(int)(playerCount * InputSize)];
            session.SetLocalDevice(0, playerCount, SimulatedFrameDelay);

            if (RollbackInfo != null) RollbackInfo.Text = "";
            if (PingInfo != null) PingInfo.Text = "";

            Spectating = false;
            Replay = true;
            Started = true;

            StartGameThread();
        }

        private void GameThreadLoop()
        {
            while (Started)
            {
                mutex.WaitOne();
                GameLoop();  
                mutex.ReleaseMutex();
                Thread.Sleep((int)((1 / 60f) * 1000));
            }
        }

        private void GameLoop()
        {
            if (!session.IsOffline()) session.Poll();

            if (session.IsRunning())
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
                            sessionState.LoadState(reader);
                            break;
                        case SessionSaveGameAction SGAction:
                            MemoryStream writerStream = new MemoryStream();
                            BinaryWriter writer = new BinaryWriter(writerStream);
                            sessionState.SaveState(writer);
                            byte[] state = writerStream.ToArray();
                            SGAction.Save(state, Platform.GetChecksum(state));
                            break;
                    }
                }

                if (Spectating)
                {
                    SimulationText = $"Spectating...";
                    RollbackText = "";
                    PingText = "";
                }
                else
                {
                    string FrameCounter = session.IsOffline() ? $"{session.Frame()}" : $"{session.Frame()} ({session.RemoteFrame()} | {session.FrameAdvantage()} | {session.RemoteFrameAdvantage()})";

                    SimulationText = FrameCounter + $" || ( {InputDebug} )";
                    if (!session.IsOffline())
                    {
                        RollbackText = "RBF: " + session.AverageRollbackFrames();
                        PingText = "Ping: " + ShowPingInfo(PingId) + " ms";
                    }
                }

                if (Replay && session.Frame() >= RecordedInputs.Count)
                    CloseGame();
            }
        }

        private void StartGameThread()
        {
            if (!UseSeparateThread) return;

            GameThread = new Thread(() => GameThreadLoop());
            GameThread.IsBackground = true;
            GameThread.Start();
        }

        private string NotificationText()
        {
            if (Spectating) return SimulationText;

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
            Spectating = false;
            Started = false;
            Replay = false;
            sessionState = null;
            if (adapter != null) adapter.Close();
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

        public virtual void OnlineGame(bool spectate, uint players, uint spectators, uint ID) {}
        public virtual void LocalGame(uint players) {}
        public virtual void ReplayMode(uint players) {}
    }

    public struct ReplayInputs
    {
        public byte[] inputs;

        public ReplayInputs()
        {
            inputs = Array.Empty<byte>();
        }

        public ReplayInputs(byte[] i)
        {
            inputs = i;
        }
    };
}
