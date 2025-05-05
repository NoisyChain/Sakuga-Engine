using Godot;
using PleaseResync;
using System;
using System.IO;


public partial class TestGameMgr : Node
{
    public class GState : IGameState
    {
        public Vector2I[] Positions = new Vector2I[2];

        public void GameLoop(byte[] playerInput)
        {
            for (int i = 0; i < Positions.Length; i++)
            {
                if ((playerInput[i] & (1 << 0)) != 0) Positions[i].X -= 250;
                if ((playerInput[i] & (1 << 1)) != 0) Positions[i].X += 250;
                if ((playerInput[i] & (1 << 2)) != 0) Positions[i].Y -= 250;
                if ((playerInput[i] & (1 << 3)) != 0) Positions[i].Y += 250;
            }
        }

        public byte[] GetLocalInput(int PlayerID, int InputSize)
        {
            throw new NotImplementedException();
        }

        public void LoadState(BinaryReader br)
        {
            Positions[0].X = br.ReadInt32();
            Positions[0].Y = br.ReadInt32();
            Positions[1].X = br.ReadInt32();
            Positions[1].Y = br.ReadInt32();
        }

        public void Render()
        {
        }

        public void SaveState(BinaryWriter bw)
        {
            bw.Write(Positions[0].X);
            bw.Write(Positions[0].Y);
            bw.Write(Positions[1].X);
            bw.Write(Positions[1].Y);
        }

        public void Setup()
        {
            Positions[0] = new Vector2I(80000, 40000);
            Positions[1] = new Vector2I(40000, 40000);
        }
    }
    GState gamestate = new();
    [Export] Node2D[] Players;
    [Export] bool Spectating = false;
    [Export] uint LocalId = 0;
    private Session _session = null;

    [Export] string[] PlayerAddresses = { "127.0.0.1", "127.0.0.1" };
    [Export] string[] SpectatorAddresses = { "127.0.0.1", "127.0.0.1" };
    [Export] int[] PlayerPorts = { 7001, 7002 };
    [Export] int[] SpectatorPorts = { 8001, 8002 };

    private bool _started = false;
    private LiteNetLibSessionAdapter _netAdapter = null;
    public override void _Ready()
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_started) return;

        _session.Poll();
        if (_session.IsRunning())
        {
            byte localInput = GetLocalInput();
            var actions = _session.AdvanceFrame([localInput]);
            foreach (var action in actions)
            {
                switch (action)
                {
                    case SessionAdvanceFrameAction AFAction:
                        gamestate.GameLoop(AFAction.Inputs);
                        break;
                    case SessionLoadGameAction LGAction:
                        MemoryStream readerStream = new MemoryStream(LGAction.Load());
                        BinaryReader reader = new BinaryReader(readerStream);
                        gamestate.LoadState(reader);
                        break;
                    case SessionSaveGameAction SGAction:
                        MemoryStream writerStream = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(writerStream);
                        gamestate.SaveState(writer);
                        byte[] state = writerStream.ToArray();
                        SGAction.Save(state, Platform.GetChecksum(state));
                        break;
                }
            }

        }
        SetPlayerPositions();
    }

    private static byte GetLocalInput()
    {
        byte input = 0;

        if (Input.IsKeyPressed(Key.Left))
            input |= 1 << 0;

        if (Input.IsKeyPressed(Key.Right))
            input |= 1 << 1;

        if (Input.IsKeyPressed(Key.Up))
            input |= 1 << 2;

        if (Input.IsKeyPressed(Key.Down))
            input |= 1 << 3;

        return input;
    }


    private void SetPlayerPositions()
    {
        Players[0].Position = gamestate.Positions[0] / 100;
        Players[1].Position = gamestate.Positions[1] / 100;
    }

    private void _on_btn_session_pressed()
    {
        if (_started)
        {
            GetNode<Button>("../UI/Container/BtnSession").Text = "Start Session";
            _started = false;
            _session = null;
            _netAdapter.Close();
            _netAdapter = null;
            return;
        }

        Spectating = GetNode<CheckBox>("../UI/Container/Spectating").ButtonPressed;
        LocalId = (uint)GetNode<SpinBox>("../UI/Container/LocalId").Value;

        uint PlayerCount = (uint)PlayerAddresses.Length;
        uint SpectatorCount = (uint)SpectatorAddresses.Length;

        gamestate.Setup();

        if (!Spectating)
        {
            _netAdapter = new LiteNetLibSessionAdapter(PlayerAddresses[LocalId], (ushort)PlayerPorts[LocalId]);
            _session = new Peer2PeerSession(sizeof(byte), 2, PlayerCount, false, _netAdapter);
            // local players  
            _session.SetLocalDevice(LocalId, 1, 0);
            // add remote players
            for (uint i = 0; i < PlayerCount; i++)
            {
                if (i != LocalId)
                {
                    _session.AddRemoteDevice(i, 1, LiteNetLibSessionAdapter.CreateRemoteConfig(PlayerAddresses[i], (ushort)PlayerPorts[i]));
                }
            }
            // add spectators
            if (LocalId == 0)
            {
                for (uint i = 0; i < SpectatorCount; i++)
                {
                    _session.AddSpectatorDevice(0, LiteNetLibSessionAdapter.CreateRemoteConfig(SpectatorAddresses[i], (ushort)SpectatorPorts[i]));
                }
            }
        }
        else
        {
            // lets spectate
            _netAdapter = new LiteNetLibSessionAdapter(SpectatorAddresses[LocalId], (ushort)SpectatorPorts[LocalId]);
            _session = new SpectatorSession(sizeof(byte), PlayerCount, _netAdapter);
            // lets just make the first active player the broadcaster for now.
            // be sure to pass ALL the players as in player count
            _session.AddRemoteDevice(0, PlayerCount,
                LiteNetLibSessionAdapter.CreateRemoteConfig(PlayerAddresses[0], (ushort)PlayerPorts[0]));
        }

        _started = true;
        GetNode<Button>("../UI/Container/BtnSession").Text = "Stop Session";
    }
}
