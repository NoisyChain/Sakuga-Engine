using Godot;
using System.IO;
using PleaseResync;
using SakugaEngine.Resources;
using SakugaEngine.Collision;
using SakugaEngine.UI;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace SakugaEngine.Game
{
    public partial class GameManager : Node, IGameState
    {
        [Export] public MatchSettings Match;
        [Export] private GameMonitor Monitor;
        [Export] public FighterList fightersList;
        [Export] public StageList stagesList;
        [Export] public BGMList songsList;
        [Export] private CanvasLayer FighterUI;
        [Export] private FighterCamera Camera;
        [Export] private AudioStreamPlayer BGMSource;
        private Label SeedViewer;
        private FadeScreen Curtain;
        public uint InputSize;

        private List<SakugaNode> Nodes = new List<SakugaNode>();
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
            SeedViewer = (Label)FighterUI.GetNode("Seed");
            Control OnlinePlayersInfo = (Control)healthHUD.GetNode("OnlineInfo");
            Control TrainingInfo = (Control)metersHUD.GetNode("TrainingInfo");
            Control InputHistory = (Control)metersHUD.GetNode("InputHistory");
            Control DebugWindow = (Control)FighterUI.GetNode("Debug");
            Control NetcodeHUD = (Control)FighterUI.GetNode("PleaseResync_UI");
            if (Monitor != null)
            {
                Monitor.Cards = (MatchCardsController)FighterUI.GetNode("MatchCards");
                Monitor.ResultsScreen = (Control)FighterUI.GetNode("ResultsScreen");
                Monitor.ResultsScreen.Visible = false;
            }
            Curtain = (FadeScreen)FighterUI.GetNode("FadeScreen");
            OnlinePlayersInfo.Visible = Match.SelectedModeSettings.ShowOnlineInfo;
            TrainingInfo.Visible = Match.SelectedModeSettings.ShowTrainingInfo;
            InputHistory.Visible = Match.SelectedModeSettings.AllowShowInputs;
            DebugWindow.Visible = Match.SelectedModeSettings.AllowShowDebugElements;
            NetcodeHUD.Visible = Match.SelectedModeSettings.AllowShowDebugElements;
            SeedViewer.Visible = Match.SelectedModeSettings.AllowShowDebugElements;

            if (Match.SelectedModeSettings.ShowOnlineInfo && OnlinePlayersInfo != null)
            {
                ((OnlineInfo)OnlinePlayersInfo).Set(Match);
            }
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (Fighters == null) return;
            if (Monitor == null) return;

            if (!BGMSource.Playing) BGMSource.Play();
            if (SeedViewer.Visible) SeedViewer.Text = finalSeed.ToString();

            DebugCommands();
        }

        public void SetBGM()
        {
            if (BGMSource != null && !BGMSource.Playing && songsList.elements[Match.SelectedBGM].clip != null)
                BGMSource.Stream = songsList.elements[Match.SelectedBGM].clip;
        }

        public void Render()
        {
            RenderNodes();
            Camera.UpdateCamera(Fighters[0], Fighters[1]);
            healthHUD.UpdateHealthBars(Fighters, Monitor);
            metersHUD.UpdateMeters(Fighters);
            Curtain.FadeIntensity = Monitor.FadeScreenIntensity;
            Monitor.Render();
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
            string seedText = Global.baseSeed + Fighters[0].Data.Profile.FighterName + Fighters[1].Data.Profile.FighterName;
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
            int stateFrame = Fighters[0].Animator.CurrentStateFrame + Fighters[0].Animator.CurrentState;
            stateFrame += Fighters[1].Animator.CurrentStateFrame + Fighters[1].Animator.CurrentState;
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

            CreateStage(Match.SelectedStage);

            World = new PhysicsWorld();
            Nodes.Clear();
            Fighters = new SakugaFighter[2];

            CreateFighter(Match.P1SelectedCharacter, 0);
            CreateFighter(Match.P2SelectedCharacter, 1);

            Fighters[0].SetOpponent(Fighters[1]);
            Fighters[1].SetOpponent(Fighters[0]);

            GenerateBaseSeed();
            Monitor.Initialize(Fighters, Match);

            healthHUD.Setup(Fighters);
            metersHUD.Setup(Fighters);
        }

        public void CreateFighter(int characterIndex, int playerIndex)
        {
            bool AIcontrolled = false;
            int selectedColor = 0;
            switch (playerIndex)
            {
                case 0:
                    AIcontrolled = !Match.SelectedModeSettings.NoAI && Match.P1SelectedDevice == -1;
                    selectedColor = Match.P1SelectedColor;
                    break;
                case 1:
                    AIcontrolled = !Match.SelectedModeSettings.NoAI && Match.P2SelectedDevice == -1;
                    selectedColor = Match.P2SelectedColor;
                    if (characterIndex == Match.P1SelectedCharacter && Match.P1SelectedColor == Match.P2SelectedColor)
                        selectedColor++;
                    if (selectedColor >= fightersList.elements[characterIndex].ColorPalettes.Length)
                        selectedColor = 0;
                    break;
            }
            
            Node temp = fightersList.elements[characterIndex].ColorPalettes[selectedColor].Instance.Instantiate();
            Fighters[playerIndex] = temp as SakugaFighter;
            AddActor(Fighters[playerIndex]);
            Fighters[playerIndex].Initialize(playerIndex);
            Fighters[playerIndex].InitializeAI(AIcontrolled, Match.Difficulty);
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
                if (!Match.SelectedModeSettings.CanKO)
                {
                    if (Fighters[i].Variables.CurrentHealth <= 0)
                        Fighters[i].Variables.CurrentHealth = 1;
                    if (!Fighters[i].HitStun.IsRunning())
                        Fighters[i].Variables.CurrentHealth = Fighters[i].Data.MaxHealth;
                }
            }
            
            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].LateTick();
        }

        private void DebugCommands()
        {
            if (Input.IsActionJustPressed("toggle_hitboxes") && Match.SelectedModeSettings.AllowShowHitboxes)
                Global.ShowHitboxes = !Global.ShowHitboxes;
            
            if (!Match.SelectedModeSettings.AllowUseDebugCommands) return;

            if (Input.IsActionJustPressed("debug_f1"))
            {
                if (Input.IsActionPressed("debug_shift"))
                    Fighters[0].Variables.CurrentHealth = 0;
                else
                    Fighters[0].Variables.CurrentHealth -= 100;
            }
            if (Input.IsActionJustPressed("debug_f2"))
            {
                if (Input.IsActionPressed("debug_shift"))
                    Fighters[1].Variables.CurrentHealth = 0;
                else
                    Fighters[1].Variables.CurrentHealth -= 100;
            }
            if (Input.IsActionJustPressed("debug_f3"))
            {
                if (Input.IsActionPressed("debug_shift"))
                    Fighters[0].Variables.CurrentHealth = Fighters[0].Data.MaxHealth;
                else
                    Fighters[0].Variables.CurrentHealth += 100;
            }
            if (Input.IsActionJustPressed("debug_f4"))
            {
                if (Input.IsActionPressed("debug_shift"))
                    Fighters[1].Variables.CurrentHealth = Fighters[1].Data.MaxHealth;
                else
                    Fighters[1].Variables.CurrentHealth += 100;
            }
            if (Input.IsActionJustPressed("debug_f5"))
            {
                Fighters[0].Variables.CurrentSuperGauge = Fighters[0].Data.MaxSuperGauge;
            }
            if (Input.IsActionJustPressed("debug_f6"))
            {
                Fighters[1].Variables.CurrentSuperGauge = Fighters[1].Data.MaxSuperGauge;
            }
            if (Input.IsActionJustPressed("debug_timer"))
            {
                if (Input.IsActionPressed("debug_shift"))
                    Monitor.ResetTimer();
                else
                    Monitor.Clock = 0;
            }
            if (Input.IsActionJustPressed("debug_reset"))
            {
                Fighters[0].Reset(0);
                Fighters[1].Reset(1);
            }
        }

        // Generate inputs for your game
        //NOTICE: for every 8 inputs you need to change the index
        public byte[] ReadInputs(int id, int inputSize)
        {
            byte[] input = new byte[inputSize];
            if (id < 0) return input;

            string prefix = Global.GetPlayerPrefix(id);

            if (Input.IsActionPressed(prefix + "_up") && !Input.IsActionPressed(prefix + "_down"))
                input[0] |= Global.INPUT_UP;

            if (!Input.IsActionPressed(prefix + "_up") && Input.IsActionPressed(prefix + "_down"))
                input[0] |= Global.INPUT_DOWN;

            if (Input.IsActionPressed(prefix + "_left") && !Input.IsActionPressed(prefix + "_right"))
                input[0] |= Global.INPUT_LEFT;

            if (!Input.IsActionPressed(prefix + "_left") && Input.IsActionPressed(prefix + "_right"))
                input[0] |= Global.INPUT_RIGHT;

            if (Input.IsActionPressed(prefix + "_face_a"))
                input[0] |= Global.INPUT_FACE_A;

            if (Input.IsActionPressed(prefix + "_face_b"))
                input[0] |= Global.INPUT_FACE_B;

            if (Input.IsActionPressed(prefix + "_face_c"))
                input[0] |= Global.INPUT_FACE_C;

            if (Input.IsActionPressed(prefix + "_face_d"))
                input[0] |= Global.INPUT_FACE_D;
            
            if (Input.IsActionPressed(prefix + "_face_e"))
                input[1] |= Global.INPUT_FACE_E >> 8;

            if (Input.IsActionPressed(prefix + "_face_f"))
                input[1] |= Global.INPUT_FACE_F >> 8;
            
            if (Input.IsActionPressed(prefix + "_face_g"))
                input[1] |= Global.INPUT_FACE_G >> 8;

            if (Input.IsActionPressed(prefix + "_face_h"))
                input[1] |= Global.INPUT_FACE_H >> 8;

            /*if (Input.IsActionPressed(prefix + "_macro_ab"))
                input |= Global.INPUT_FACE_A | Global.INPUT_FACE_B;

            if (Input.IsActionPressed(prefix + "_macro_ac"))
                input |= Global.INPUT_FACE_A | Global.INPUT_FACE_C;
            
            if (Input.IsActionPressed(prefix + "_macro_bc"))
                input |= Global.INPUT_FACE_B | Global.INPUT_FACE_C;

            if (Input.IsActionPressed(prefix + "_macro_abc"))
                input |= Global.INPUT_FACE_A | Global.INPUT_FACE_B | Global.INPUT_FACE_C;

            if (Input.IsActionPressed(prefix + "_macro_abcd"))
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
            int playerInputDevice = 0;
            if (PlayerID == 0) playerInputDevice = Match.P1SelectedDevice;
            else if (PlayerID == 1) playerInputDevice = Match.P2SelectedDevice;
            byte[] result = ReadInputs(playerInputDevice, InputSize);

            return result;
        }
    }
}
