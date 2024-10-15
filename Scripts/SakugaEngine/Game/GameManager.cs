using Godot;
using System.IO;
using PleaseResync;
using SakugaEngine.Collision;
using SakugaEngine.UI;
using System.Text;

namespace SakugaEngine.Game
{
    public partial class GameManager : Node, IGameState
    {
        [Export] public PackedScene[] Spawns;
        [Export] private CanvasLayer FighterUI;
        [Export] private FighterCamera Camera;
        [Export] Label SeedViewer;
        public uint InputSize;

        private SakugaFighter[] Fighters;
        private PhysicsWorld World;
        private GameMonitor Monitor;
        private HealthHUD healthHUD;
        private MetersHUD metersHUD;

        private int Frame = 0;
        private int generatedSeed = 0;
        //private uint Checksum;

        Vector3I randomTest = new Vector3I();

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
            healthHUD.UpdateDebug(Fighters);
            metersHUD.UpdateMeters(Fighters);
            Camera.UpdateCamera(Fighters[0], Fighters[1]);
            SeedViewer.Text = CalculateSeed().ToString();
        }

        private void GenerateBaseSeed()
        {
            byte[] seedArray = Encoding.ASCII.GetBytes(Global.baseSeed);
            generatedSeed = (int)Platform.GetChecksum(seedArray);
        }

        private int CalculateSeed()
        {
            int posX = Fighters[0].Body.FixedPosition.X + Fighters[1].Body.FixedPosition.X;
            int posY = Fighters[0].Body.FixedPosition.Y + Fighters[1].Body.FixedPosition.Y;
            int stateFrame = Fighters[0].Animator.Frame + Fighters[0].Animator.CurrentState + Fighters[1].Animator.Frame + Fighters[1].Animator.CurrentState;
            int charSeeds = Fighters[0].CharacterSeed + Fighters[1].CharacterSeed;
            return generatedSeed + posX + posY + stateFrame + charSeeds + (Frame * Global.SimulationScale) + Monitor.Clock;
        }

        public void Setup()
        {
            if (GetChildren().Count > 0)
            {
                foreach (Node child in GetChildren())
                {
                    child.QueueFree();
                }
            }

            Frame = 0;

            Monitor = new GameMonitor(Global.GameTimer, 2);
            World = new PhysicsWorld();
            GenerateBaseSeed();

            Fighters = new SakugaFighter[2];
            for (int i = 0; i < Spawns.Length; i++)
            {
                Node temp = Spawns[i].Instantiate();
                AddChild(temp);
                Fighters[i] = temp as SakugaFighter;
                World.AddBody(Fighters[i].Body);
                Fighters[i].Initialize(i);
                Fighters[i].SpawnablesSetup(this, World);
                Fighters[i].VFXSetup(this);
            }

            Fighters[0].SetOpponent(Fighters[1]);
            Fighters[1].SetOpponent(Fighters[0]);

            healthHUD.Setup(Fighters);
            metersHUD.Setup(Fighters);
        }

        public void GameLoop(byte[] playerInput)
        {
            Frame++;
            Global.UpdateRNG(CalculateSeed());

            randomTest = new Vector3I(
                Global.RNG.Next(),
                Global.RNG.Next(),
                Global.RNG.Next()
            );

            int center = (Fighters[0].Body.FixedPosition.X + Fighters[1].Body.FixedPosition.X) / 2;

            if (Fighters[0].Body.FixedPosition.X < Fighters[1].Body.FixedPosition.X)
            { Fighters[0].UpdateSide(true); Fighters[1].UpdateSide(false); }
            else if (Fighters[0].Body.FixedPosition.X > Fighters[1].Body.FixedPosition.X)
            { Fighters[0].UpdateSide(false); Fighters[1].UpdateSide(true); }

            for (int i = 0; i < Fighters.Length; i++)
            {
                ushort combinedInput = 0;
                combinedInput |= playerInput[i * InputSize];
                combinedInput |= (ushort)(playerInput[(i * InputSize) + 1] << 8);
                Fighters[i].ParseInputs(combinedInput);
            }
            
            for (int i = 0; i < Fighters.Length; i++)
                Fighters[i].Tick();
            
            World.Simulate();

            for (int i = 0; i < Fighters.Length; i++)
            {
                Fighters[i].Body.FixedPosition.X = Mathf.Clamp(
                    Fighters[i].Body.FixedPosition.X,
                    center - Global.MaxPlayersDistance / 2,
                    center + Global.MaxPlayersDistance / 2
                );
            }

            for (int i = 0; i < Fighters.Length; i++)
                Fighters[i].LateTick();

            Monitor.Tick(Fighters); 
        }

        // Generate inputs for your game
        //NOTICE: for every 8 inputs you need to change the index
        public byte[] ReadInputs(int id, int inputSize)
        {
            byte[] input = new byte[inputSize];
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
                    input[0] |= Global.INPUT_UP;

                if (!Input.IsActionPressed(prexif + "_up") && Input.IsActionPressed(prexif + "_down"))
                    input[0] |= Global.INPUT_DOWN;

                if (Input.IsActionPressed(prexif + "_left") && !Input.IsActionPressed(prexif + "_right"))
                    input[0] |= Global.INPUT_LEFT;

                if (!Input.IsActionPressed(prexif + "_left") && Input.IsActionPressed(prexif + "_right"))
                    input[0] |= Global.INPUT_RIGHT;

                if (Input.IsActionPressed(prexif + "_face_a"))
                    input[0] |= Global.INPUT_FACE_A;

                if (Input.IsActionPressed(prexif + "_face_b"))
                    input[0] |= Global.INPUT_FACE_B;

                if (Input.IsActionPressed(prexif + "_face_c"))
                    input[0] |= Global.INPUT_FACE_C;

                if (Input.IsActionPressed(prexif + "_face_d"))
                    input[0] |= Global.INPUT_FACE_D;

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
            
            bw.Write(randomTest.X);
            bw.Write(randomTest.Y);
            bw.Write(randomTest.Z);
        }

        public void Deserialize(BinaryReader br)
        {
            Frame = br.ReadInt32();
            Monitor.Deserialize(br);
            for (int i = 0; i < Fighters.Length; i++)
                Fighters[i].Deserialize(br);
            
            randomTest.X = br.ReadInt32();
            randomTest.Y = br.ReadInt32();
            randomTest.Z = br.ReadInt32();
        }

        public byte[] GetLocalInput(int PlayerID, int InputSize)
        {
            byte[] result = ReadInputs(PlayerID, InputSize);

            return result;
        }
    }
}
