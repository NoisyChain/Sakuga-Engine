using Godot;
using PleaseResync;
using SakugaEngine.Resources;
using SakugaEngine.Collision;
using SakugaEngine.UI;
using System.Text;
using System.Collections.Generic;
using SakugaEngine.Global;
using MessagePack;
using SakugaEngine.GameState;
using System;

namespace SakugaEngine.Game
{
    public partial class GameManager : Node, IGameState
    {
        public static GameManager Instance;
        public InputManager[] Inputs = [];
        [Export] public MatchSettings Match;
        [Export] public GameMonitor Monitor;
        [Export] public FighterList fightersList;
        [Export] public StageList stagesList;
        [Export] public BGMList songsList;
        [Export] public CanvasLayer FighterUI;
        [Export] public FighterCamera Camera;
        [Export] public AudioStreamPlayer BGMSource;
        private Label SeedViewer;
        private FadeScreen Curtain;
        public uint InputSize;

        public List<SakugaNode> Nodes = new List<SakugaNode>();
        private SakugaActor[] Fighters;
        private PhysicsWorld World;
        
        private HealthHUD healthHUD;
        private MetersHUD metersHUD;

        public int Frame = 0;

        //Random Number Generator
        private int generatedSeed = 0;
        private int finalSeed = 0;
        public bool ShowHitboxes;

        public Vector3I randomTest = new Vector3I();

        private GameManagerState State;
        
        public override void _Ready()
        {
            Instance = this;
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
            string seedText = RNG.baseSeed + Fighters[0].Data.Profile.FighterName + Fighters[1].Data.Profile.FighterName;
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
            int stateFrame = Fighters[0].StateManager.CurrentStateFrame + Fighters[0].StateManager.CurrentState;
            stateFrame += Fighters[1].StateManager.CurrentStateFrame + Fighters[1].StateManager.CurrentState;
            return generatedSeed + posX + posY + stateFrame + (Frame * GlobalVariables.SimulationScale) + Monitor.Clock;
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

            Inputs = new InputManager[2];
            Inputs[0] = new InputManager();
            Inputs[1] = new InputManager();

            Frame = 0;

            CreateStage(Match.SelectedStage);

            World = new PhysicsWorld();
            Nodes.Clear();
            Fighters = new SakugaActor[2];

            CreateFighter(Match.P1SelectedCharacter, 0);
            CreateFighter(Match.P2SelectedCharacter, 1);

            Fighters[0].SetAllies([Fighters[0]]);
            Fighters[1].SetAllies([Fighters[1]]);
            Fighters[0].SetOpponents([Fighters[1]]);
            Fighters[1].SetOpponents([Fighters[0]]);

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
            Fighters[playerIndex] = temp as SakugaActor;
            AddActor(Fighters[playerIndex]);
            Fighters[playerIndex].playerID = (uint)playerIndex;
            Fighters[playerIndex].Inputs = Inputs[playerIndex];
            Fighters[playerIndex].Initialize();
            Fighters[playerIndex].InitializeBrain(AIcontrolled, Match.Difficulty);
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
            RNG.Set(finalSeed);
            Frame++;
            Monitor.Tick();

            randomTest = new Vector3I(
                RNG.Next(),
                RNG.Next(),
                RNG.Next()
            );

            int center = (Fighters[0].Body.FixedPosition.X + Fighters[1].Body.FixedPosition.X) / 2;

            for (int i = 0; i < Fighters.Length; i++)
            {
                if (Monitor.MatchState == MatchState.ROUND_END)
                {
                    Inputs[i].InsertToHistory(0);
                    continue;
                }
                if (Fighters[i].UseAI)
                {
                    Fighters[i].Brain.SelectCommand();
                    Fighters[i].Brain.UpdateCommand();
                    continue;
                }

                PlayerInputs combinedInput = 0;
                combinedInput |= (PlayerInputs)playerInput[i * InputSize];
                combinedInput |= (PlayerInputs)(playerInput[(i * InputSize) + 1] << 8);
                Inputs[i].InsertToHistory(combinedInput);
            }

            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].PreTick();

            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].Tick();

            World.Simulate();

            if (Fighters[0].Body.FixedPosition.X < Fighters[1].Body.FixedPosition.X)
            { Fighters[0].Body.UpdateSide(true); Fighters[1].Body.UpdateSide(false); }
            else if (Fighters[0].Body.FixedPosition.X > Fighters[1].Body.FixedPosition.X)
            { Fighters[0].Body.UpdateSide(false); Fighters[1].Body.UpdateSide(true); }

            for (int i = 0; i < Fighters.Length; i++)
            {
                Fighters[i].Body.FixedPosition.X = Mathf.Clamp(
                    Fighters[i].Body.FixedPosition.X,
                    center - GlobalVariables.MaxPlayersDistance / 2,
                    center + GlobalVariables.MaxPlayersDistance / 2
                );
                Fighters[i].Body.UpdateColliders();
                if (!Match.SelectedModeSettings.CanKO)
                {
                    if (Fighters[i].Parameters.Health.CurrentValue <= 0)
                        Fighters[i].Parameters.Health.CurrentValue = 1;
                    if (!Fighters[i].Hitstun.IsRunning())
                        Fighters[i].Parameters.Health.CurrentValue = Fighters[i].Data.MaxHealth;
                }
            }
            
            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].LateTick();
        }

        private void DebugCommands()
        {
            if (Input.IsActionJustPressed("toggle_hitboxes") && Match.SelectedModeSettings.AllowShowHitboxes)
                ShowHitboxes = !ShowHitboxes;
            
            if (!Match.SelectedModeSettings.AllowUseDebugCommands) return;

            if (Input.IsActionJustPressed("debug_f1"))
            {
                if (Input.IsActionPressed("debug_shift"))
                    Fighters[0].Parameters.Health.SetHealth(0);
                else
                    Fighters[0].Parameters.Health.RemoveHealth(100);
            }
            if (Input.IsActionJustPressed("debug_f2"))
            {
                if (Input.IsActionPressed("debug_shift"))
                    Fighters[1].Parameters.Health.SetHealth(0);
                else
                    Fighters[1].Parameters.Health.RemoveHealth(100);
            }
            if (Input.IsActionJustPressed("debug_f3"))
            {
                if (Input.IsActionPressed("debug_shift"))
                    Fighters[0].Parameters.Health.SetHealth(Fighters[0].Data.MaxHealth);
                else
                    Fighters[0].Parameters.Health.AddHealth(100);
            }
            if (Input.IsActionJustPressed("debug_f4"))
            {
                if (Input.IsActionPressed("debug_shift"))
                    Fighters[1].Parameters.Health.SetHealth(Fighters[1].Data.MaxHealth);
                else
                    Fighters[1].Parameters.Health.AddHealth(100);
            }
            if (Input.IsActionJustPressed("debug_f5"))
            {
                Fighters[0].Parameters.SuperGauge.SetSuperGauge(Fighters[0].Data.MaxSuperGauge);
            }
            if (Input.IsActionJustPressed("debug_f6"))
            {
                Fighters[1].Parameters.SuperGauge.SetSuperGauge(Fighters[1].Data.MaxSuperGauge);
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
                Fighters[0].Reset();
                Fighters[1].Reset();
            }
        }

        // Generate inputs for your game
        //NOTICE: for every 8 inputs you need to change the index
        public byte[] ReadInputs(int id, int inputSize)
        {
            if (id < 0) return new byte[inputSize];

            PlayerInputs input = 0;

            string prefix = GlobalFunctions.GetPlayerPrefix(id);

            if (Input.IsActionPressed(prefix + "_up") && !Input.IsActionPressed(prefix + "_down"))
                input |= PlayerInputs.UP;

            if (!Input.IsActionPressed(prefix + "_up") && Input.IsActionPressed(prefix + "_down"))
                input |= PlayerInputs.DOWN;

            if (Input.IsActionPressed(prefix + "_left") && !Input.IsActionPressed(prefix + "_right"))
                input |= PlayerInputs.LEFT;

            if (!Input.IsActionPressed(prefix + "_left") && Input.IsActionPressed(prefix + "_right"))
                input |= PlayerInputs.RIGHT;

            if (Input.IsActionPressed(prefix + "_face_a"))
                input |= PlayerInputs.FACE_A;

            if (Input.IsActionPressed(prefix + "_face_b"))
                input |= PlayerInputs.FACE_B;

            if (Input.IsActionPressed(prefix + "_face_c"))
                input |= PlayerInputs.FACE_C;

            if (Input.IsActionPressed(prefix + "_face_d"))
                input |= PlayerInputs.FACE_D;
            
            if (Input.IsActionPressed(prefix + "_face_e"))
                input |= PlayerInputs.FACE_E;

            if (Input.IsActionPressed(prefix + "_face_f"))
                input |= PlayerInputs.FACE_F;

            if (Input.IsActionPressed(prefix + "_face_g"))
                input |= PlayerInputs.FACE_G;

            if (Input.IsActionPressed(prefix + "_face_h"))
                input |= PlayerInputs.FACE_H;

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
        
            return BitConverter.GetBytes((ushort)input);
        }

        public byte[] SaveState()
        {
            State.GetStateData(ref Instance);
            return MessagePackSerializer.Serialize(State);
        }

        public void LoadState(byte[] stateBuffer)
        {
            State = MessagePackSerializer.Deserialize<GameManagerState>(stateBuffer);
            State.SetStateData(ref Instance);
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
