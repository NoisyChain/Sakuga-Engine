using Godot;
using System.IO;
using PleaseResync;
using SakugaEngine.Resources;
using SakugaEngine.Collision;
using SakugaEngine.UI;
using System.Text;
using System.Collections.Generic;

namespace SakugaEngine.Game
{
    public partial class GameManager : Node, IGameState
    {
        [Export] private GameMonitor Monitor;
        [Export] public FighterList fightersList;
        [Export] public StageList stagesList;
        [Export] public BGMList songsList;
        [Export] public int player1Character;
        [Export] public int player2Character;
        [Export] public int selectedStage;
        [Export] public int selectedBGM;
        [Export] private CanvasLayer FighterUI;
        [Export] private FighterCamera Camera;
        [Export] private AudioStreamPlayer BGMSource;
        [Export] Label SeedViewer;
        public uint InputSize;

        private List<SakugaNode> Nodes;
        private SakugaFighter[] Fighters;
        private PhysicsWorld World;
        
        private HealthHUD healthHUD;
        private MetersHUD metersHUD;

        private int Frame = 0;
        private int generatedSeed = 0;
        private int finalSeed = 0;

        Vector3I randomTest = new Vector3I();
        
        public override void _Ready()
        {
            healthHUD = (HealthHUD)FighterUI.GetNode("GameHUD_Background");
            metersHUD = (MetersHUD)FighterUI.GetNode("GameHUD_Foreground");
            Nodes = new List<SakugaNode>();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (Fighters == null) return;
            if (Monitor == null) return;

            if (!BGMSource.Playing) BGMSource.Play();
            SeedViewer.Text = finalSeed.ToString();

            if (Input.IsActionJustPressed("toggle_hitboxes"))
                Global.ShowHitboxes = !Global.ShowHitboxes;
        }

        public void SetBGM()
        {
            if (BGMSource != null && !BGMSource.Playing && songsList.elements[selectedBGM].clip != null)
                BGMSource.Stream = songsList.elements[selectedBGM].clip;
        }

        public void Render()
        {
            RenderNodes();
            Camera.UpdateCamera(Fighters[0], Fighters[1]);
            healthHUD.UpdateHealthBars(Fighters, Monitor);
            metersHUD.UpdateMeters(Fighters);
        }

        void RenderNodes()
        {
            foreach (SakugaNode node in Nodes)
            {
                node.Render();
            }
        }

        /// <summary>
        /// Generates the base seed to be used for generating the PRNG. The base seed is a non-random 
        /// number generated with a default string and both characters' names.
        /// </summary>
        private void GenerateBaseSeed()
        {
            string seedText = Global.baseSeed + Fighters[0].Profile.FighterName + Fighters[1].Profile.FighterName;
            byte[] seedArray = Encoding.ASCII.GetBytes(seedText);
            generatedSeed = (int)Platform.GetChecksum(seedArray);
        }

        /// <summary>
        /// Generate the PRNG seed with a bunch of everchanging values. 
        /// If the values used are deterministic, the generated seed will be deterministic.
        /// </summary>
        /// <returns>a 32-bit seed number</returns>
        private int CalculateSeed()
        {
            int posX = Fighters[0].Body.FixedPosition.X + Fighters[1].Body.FixedPosition.X;
            int posY = Fighters[0].Body.FixedPosition.Y + Fighters[1].Body.FixedPosition.Y;
            int stateFrame = Fighters[0].Animator.Frame + Fighters[0].Animator.CurrentState + Fighters[1].Animator.Frame + Fighters[1].Animator.CurrentState;
            return generatedSeed + posX + posY + stateFrame + (Frame * Global.SimulationScale) + Monitor.Clock;
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

            CreateStage(selectedStage);

            World = new PhysicsWorld();
            Nodes.Clear();
            Fighters = new SakugaFighter[2];

            CreateFighter(player1Character, 0);
            CreateFighter(player2Character, 1);

            Fighters[0].SetOpponent(Fighters[1]);
            Fighters[1].SetOpponent(Fighters[0]);

            //AI test (select it in a better way later)
            //Fighters[0].UseAI = Global.Match.Player1.selectedDevice == -1;
            //Fighters[1].UseAI = Global.Match.Player2.selectedDevice == -1;

            GenerateBaseSeed();
            Monitor.Initialize(Fighters);

            healthHUD.Setup(Fighters);
            metersHUD.Setup(Fighters);
        }

        public void CreateFighter(int characterIndex, int playerIndex)
        {
            Node temp = fightersList.elements[characterIndex].Instance.Instantiate();
            Fighters[playerIndex] = temp as SakugaFighter;
            AddActor(Fighters[playerIndex]);
            Fighters[playerIndex].Initialize(playerIndex);
            Fighters[playerIndex].SpawnablesSetup(this);
            Fighters[playerIndex].VFXSetup(this);
        }

        public void CreateStage(int stageIndex)
        {
            Node temp = stagesList.elements[stageIndex].Instance.Instantiate();
            AddChild(temp);
        }

        public void AddActor(SakugaNode newNode, bool isPhysicsBody = true)
        {
            AddChild(newNode);
            Nodes.Add(newNode);
            if (isPhysicsBody) World.AddBody((newNode as SakugaActor).Body);
        }

        public void GameLoop(byte[] playerInput)
        {
            finalSeed = CalculateSeed();
            Global.UpdateRNG(finalSeed);
            Frame++;
            Monitor.Tick();

            randomTest = new Vector3I(
                Global.RNG.Next(),
                Global.RNG.Next(),
                Global.RNG.Next()
            );

            int center = (Fighters[0].Body.FixedPosition.X + Fighters[1].Body.FixedPosition.X) / 2;

            for (int i = 0; i < Fighters.Length; i++)
            {
                if (Fighters[i].UseAI)
                {
                    Fighters[i].Brain.SelectCommand();
                    Fighters[i].Brain.UpdateCommand();
                }
                else
                {
                    ushort combinedInput = 0;
                    combinedInput |= playerInput[i * InputSize];
                    combinedInput |= (ushort)(playerInput[(i * InputSize) + 1] << 8);
                    Fighters[i].ParseInputs(combinedInput);
                }
            }

            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].PreTick();

            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].Tick();

            World.Simulate();

            if (Fighters[0].Body.FixedPosition.X < Fighters[1].Body.FixedPosition.X)
            { Fighters[0].UpdateSide(true); Fighters[1].UpdateSide(false); }
            else if (Fighters[0].Body.FixedPosition.X > Fighters[1].Body.FixedPosition.X)
            { Fighters[0].UpdateSide(false); Fighters[1].UpdateSide(true); }

            for (int i = 0; i < Fighters.Length; i++)
            {
                Fighters[i].Body.FixedPosition.X = Mathf.Clamp(
                    Fighters[i].Body.FixedPosition.X,
                    center - Global.MaxPlayersDistance / 2,
                    center + Global.MaxPlayersDistance / 2
                );
                Fighters[i].Body.UpdateColliders();
            }
            
            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].LateTick();
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

        public void SaveState(BinaryWriter bw)
        {
            bw.Write(Frame);
            Monitor.Serialize(bw);
            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].Serialize(bw);
            
            bw.Write(randomTest.X);
            bw.Write(randomTest.Y);
            bw.Write(randomTest.Z);
        }

        public void LoadState(BinaryReader br)
        {
            Frame = br.ReadInt32();
            Monitor.Deserialize(br);
            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].Deserialize(br);
            
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
