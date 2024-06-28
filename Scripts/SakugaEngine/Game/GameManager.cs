using Godot;
using System.IO;
using PleaseResync;
using SakugaEngine.Collision;
using SakugaEngine.UI;

namespace SakugaEngine.Game
{
    public partial class GameManager : Node, IGameState
    {
        [Export] public PackedScene[] Spawns;
        [Export] private CanvasLayer FighterUI;
        [Export] private FighterCamera Camera;

        private FighterBody[] Fighters;
        private PhysicsWorld World;
        private GameMonitor Monitor;
        private HealthHUD healthHUD;
        private MetersHUD metersHUD;

        private int Frame = -1;
        //private uint Checksum;

        public override void _Ready()
        {
            healthHUD = (HealthHUD)FighterUI.GetNode("GameHUD_Background");
            metersHUD = (MetersHUD)FighterUI.GetNode("GameHUD_Foreground");
        }

        public override void _Process(double delta)
        {
            if (Fighters == null) return;
            if (Monitor == null) return;

            healthHUD.UpdateHealthBars(Fighters, Monitor.VictoryCounter);
            healthHUD.UpdateTimer(Monitor.Clock / Global.TicksPerSecond);
            metersHUD.UpdateMeters(Fighters);
            Camera.UpdateCamera(Fighters[0], Fighters[1]);
        }

        public void Setup()
        {
            Monitor = new GameMonitor(Global.GameTimer, 2);
            World = new PhysicsWorld();

            Fighters = new FighterBody[2];
            for (int i = 0; i < Spawns.Length; i++)
            {
                Node temp = Spawns[i].Instantiate();
                AddChild(temp);
                Fighters[i] = temp as FighterBody;
                World.AddBody(Fighters[i].Body);
                Fighters[i].Initialize(Global.StartingPosition * (-1 + (i * 2)));
            }

            Fighters[0].SetOpponent(Fighters[1]);
            Fighters[1].SetOpponent(Fighters[0]);

            healthHUD.Setup(Fighters);
            metersHUD.Setup(Fighters);
        }

        public void GameLoop(PlayerInputs[] playerInput)
        {
            Frame++;

            int center = (Fighters[0].Body.FixedPosition.X + Fighters[1].Body.FixedPosition.X) / 2;

            if (Fighters[0].Body.FixedPosition.X < Fighters[1].Body.FixedPosition.X)
            { Fighters[0].ChangeSide(true); Fighters[1].ChangeSide(false); }
            else if (Fighters[0].Body.FixedPosition.X > Fighters[1].Body.FixedPosition.X)
            { Fighters[0].ChangeSide(false); Fighters[1].ChangeSide(true); }

            for (int i = 0; i < Fighters.Length; i++)
                Fighters[i].ParseInputs(playerInput[i].rawInput);
            
            for (int i = 0; i < Fighters.Length; i++)
                Fighters[i].Tick();
            
            World.Simulate();
            Monitor.Tick(Fighters);
        }

        public ushort ReadInputs(int id)
        {
            ushort input = 0;
            string prexif = "";

            switch (id)
            {
                case 0:
                    prexif = "k1";
                    break;
                case 1:
                    prexif = "k2";
                    break;
            }

            if (Input.IsActionPressed(prexif + "_up") && !Input.IsActionPressed(prexif + "_down"))
                    input |= Global.INPUT_UP;

                if (!Input.IsActionPressed(prexif + "_up") && Input.IsActionPressed(prexif + "_down"))
                    input |= Global.INPUT_DOWN;

                if (Input.IsActionPressed(prexif + "_left") && !Input.IsActionPressed(prexif + "_right"))
                    input |= Global.INPUT_LEFT;

                if (!Input.IsActionPressed(prexif + "_left") && Input.IsActionPressed(prexif + "_right"))
                    input |= Global.INPUT_RIGHT;

                if (Input.IsActionPressed(prexif + "_face_a"))
                    input |= Global.INPUT_FACE_A;

                if (Input.IsActionPressed(prexif + "_face_b"))
                    input |= Global.INPUT_FACE_B;

                if (Input.IsActionPressed(prexif + "_face_c"))
                    input |= Global.INPUT_FACE_C;

                if (Input.IsActionPressed(prexif + "_face_d"))
                    input |= Global.INPUT_FACE_D;

                /*if (Input.IsActionPressed(prexif + "_macro_ab"))
                    input |= Global.INPUT_FACE_A | Global.INPUT_FACE_B;

                if (Input.IsActionPressed(prexif + "_macro_ac"))
                    input |= Global.INPUT_FACE_A | Global.INPUT_FACE_C;
                
                if (Input.IsActionPressed(prexif + "_macro_bc"))
                    input |= Global.INPUT_FACE_B | Global.INPUT_FACE_C;

                if (Input.IsActionPressed(prexif + "_macro_abc"))
                    input |= Global.INPUT_FACE_A | Global.INPUT_FACE_B | Global.INPUT_FACE_C;

                if (Input.IsActionPressed(prexif + "_macro_abcd"))
                    input |= Global.INPUT_FACE_A | Global.INPUT_FACE_B | Global.INPUT_FACE_C | Global.INPUT_FACE_D;*/
            

            return input;
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(Frame);
            Monitor.Serialize(bw);
            for (int i = 0; i < Fighters.Length; i++)
                Fighters[i].Serialize(bw);
        }

        public void Deserialize(BinaryReader br)
        {
            Frame = br.ReadInt32();
            Monitor.Deserialize(br);
            for (int i = 0; i < Fighters.Length; i++)
                Fighters[i].Deserialize(br);
        }

        public PlayerInputs GetLocalInput(int PlayerID)
        {
            PlayerInputs result = new PlayerInputs();
            result.rawInput = ReadInputs(PlayerID);

            return result;
        }

        public int StateFrame() => Frame;
        //public uint StateChecksum() => Checksum;
    }
}
