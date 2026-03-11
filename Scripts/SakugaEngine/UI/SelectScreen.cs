using Godot;
using SakugaEngine;
using SakugaEngine.Resources;

namespace SakugaEngine.UI
{
    public partial class SelectScreen : Control
    {
        [ExportCategory("Settings")]
        [Export] private MatchSettings Match;
        [Export] private FighterList fightersList;
        [Export] private StageList stagesList;
        [Export] private BGMList songsList;
        [Export] private Global.CharacterSelectStyle PlayerSelection;
        [Export] private Global.CharacterSelectMode SelectionMode;
        [Export] private Control CharacterSelectMode;
        [Export] private Control StageSelectMode;
        [Export] private int P1Selected = 0;
        [Export] private int P2Selected = 1;
        [Export] private int P1ColorSelected = 0;
        [Export] private int P2ColorSelected = 0;
        [Export] private int StageSelected = -2;
        [Export] private int BGMSelected = -2;
        [Export] private bool SinglePlayerSelection;
        [Export] private bool AllowSelectStage = true;
        [Export] private string destination = "res://Scenes/TestScene.tscn";
        [Export] private string returnTo = "res://Scenes/MainMenu.tscn";

        [ExportCategory("Character Select")]
        
        [Export] private TextureRect P1SelectedRender;
        [Export] private TextureRect P2SelectedRender;
        [Export] private Label P1SelectedName;
        [Export] private Label P2SelectedName;
        [Export] private TextureRect P1Cursor;
        [Export] private TextureRect P2Cursor;
        [Export] private PackedScene charactersButtonElement;
        [Export] private GridContainer charactersContainer;
        [Export] private Texture2D randomCharPortrait;
        [Export] private Texture2D randomCharRender;
        [Export] private ColorSelectMenu P1ColorSelect;
        [Export] private ColorSelectMenu P2ColorSelect;
        
        [ExportCategory("Stage Select")]
        [Export] private TextureRect StageSelectedRender;
        [Export] private Label StageSelectedName;
        [Export] private Label SongSelectedName;
        [Export] private Control P1SelectingStage;
        [Export] private Control P2SelectingStage;
        [Export] private Control StageCursor;
        [Export] private PackedScene stagesButtonElement;
        [Export] private HBoxContainer stagesContainer;
        [Export] private Texture2D randomStageThumbnail;
        [Export] private Texture2D autoStageThumbnail;

        //Hidden variables
        private Global.CharacterSelectState P1State;
        private Global.CharacterSelectState P2State;
        private bool P1Finished => P1State == Global.CharacterSelectState.DONE;
        private bool P2Finished => P2State == Global.CharacterSelectState.DONE;
        private bool isPlayer1SelectingStage = true;
        private bool AllSet = false;
        private TextureRect[] characterButtons;
        private TextureRect[] stageButtons;

        private System.Random randomSelection;

        string p1Prefix;
        string p2Prefix;

        bool P1Up, P1Down, P1Left, P1Right, P1Confirm, P1Return;
        bool P2Up, P2Down, P2Left, P2Right, P2Confirm, P2Return;

        public override void _Ready()
        {
            base._Ready();

            randomSelection = new System.Random();

            if (Match.P1SelectedDevice > -1 && Match.P2SelectedDevice > -1) PlayerSelection = Global.CharacterSelectStyle.VERSUS;
            else if (Match.P2SelectedDevice == -1) PlayerSelection = Global.CharacterSelectStyle.PLAYER1;
            else if (Match.P1SelectedDevice == -1) PlayerSelection = Global.CharacterSelectStyle.PLAYER2;

            p1Prefix = Global.GetPlayerPrefix(Match.P1SelectedDevice);
            p2Prefix = Global.GetPlayerPrefix(Match.P2SelectedDevice);

            characterButtons = new TextureRect[fightersList.elements.Length + 1];
            for (int i = 0; i <= fightersList.elements.Length; i++)
            {
                TextureRect temp = charactersButtonElement.Instantiate() as TextureRect;
                if (i == fightersList.elements.Length)
                {
                    temp.Name = "Random_Portrait";
                    temp.Texture = randomCharPortrait;
                }
                else
                {
                    temp.Name = fightersList.elements[i].Profile.ShortName + "_Portrait";
                    temp.Texture = fightersList.elements[i].Profile.Portrait;
                }
                characterButtons[i] = temp;
                charactersContainer.AddChild(temp);
            }

            stageButtons = new TextureRect[stagesList.elements.Length + 2];
            for (int i = -2; i < stagesList.elements.Length; i++)
            {
                TextureRect temp = stagesButtonElement.Instantiate() as TextureRect;
                if (i == -2)//Auto
                {
                    temp.Name = "Auto_Portrait";
                    temp.Texture = autoStageThumbnail;
                }
                else if (i == -1)//Random
                {
                    temp.Name = "Random_Portrait";
                    temp.Texture = randomStageThumbnail;
                }
                else if (i >= 0)
                {
                    temp.Name = stagesList.elements[i].Name + "_Portrait";
                    temp.Texture = stagesList.elements[i].Thumbnail;
                }
                stageButtons[i + 2] = temp;
                stagesContainer.AddChild(temp);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!Visible) return;
            if (AllSet) return;
            base._PhysicsProcess(delta);

            string p1SelectPrefix = PlayerSelection != Global.CharacterSelectStyle.PLAYER2 ? p1Prefix : p2Prefix;
            string p2SelectPrefix = PlayerSelection != Global.CharacterSelectStyle.PLAYER1 ? p2Prefix : p1Prefix;
            
            //Player 1 inputs
            P1Up = Input.IsActionJustPressed(p1SelectPrefix + "_up");
            P1Down = Input.IsActionJustPressed(p1SelectPrefix + "_down");
            P1Left = Input.IsActionJustPressed(p1SelectPrefix + "_left");
            P1Right = Input.IsActionJustPressed(p1SelectPrefix + "_right");
            P1Confirm = Input.IsActionJustPressed(p1SelectPrefix + "_accept");
            P1Return = Input.IsActionJustPressed(p1SelectPrefix + "_return");
            //Player 2 inputs
            P2Up = Input.IsActionJustPressed(p2SelectPrefix + "_up");
            P2Down = Input.IsActionJustPressed(p2SelectPrefix + "_down");
            P2Left = Input.IsActionJustPressed(p2SelectPrefix + "_left");
            P2Right = Input.IsActionJustPressed(p2SelectPrefix + "_right");
            P2Confirm = Input.IsActionJustPressed(p2SelectPrefix + "_accept");
            P2Return = Input.IsActionJustPressed(p2SelectPrefix + "_return");

            switch (SelectionMode)
            {
                case Global.CharacterSelectMode.CHARACTER_SELECT:
                    //Player 2 character selection
                    switch (P2State)
                    {
                        case Global.CharacterSelectState.SELECTING_CHARACTER:
                            SelectCharacterP2();
                            break;
                        case Global.CharacterSelectState.SELECTING_COLOR:
                            SelectColorP2();
                            break;
                        case Global.CharacterSelectState.DONE:
                            if (P2Return)
                                P2State = Global.CharacterSelectState.SELECTING_CHARACTER;
                            break;
                    }
                    //Player 1 character selection
                    switch (P1State)
                    {
                        case Global.CharacterSelectState.SELECTING_CHARACTER:
                            SelectCharacterP1();
                            break;
                        case Global.CharacterSelectState.SELECTING_COLOR:
                            SelectColorP1();
                            break;
                        case Global.CharacterSelectState.DONE:
                            if (P1Return)
                                P1State = Global.CharacterSelectState.SELECTING_CHARACTER;
                            break;
                    }
                    break;
                case Global.CharacterSelectMode.STAGE_SELECT:
                    SelectStage();
                    break;
            }

            SelectionMode = P1Finished && P2Finished ? Global.CharacterSelectMode.STAGE_SELECT : Global.CharacterSelectMode.CHARACTER_SELECT;
            CharacterSelectMode.Visible = !AllowSelectStage || SelectionMode == Global.CharacterSelectMode.CHARACTER_SELECT;
            StageSelectMode.Visible = AllowSelectStage && SelectionMode == Global.CharacterSelectMode.STAGE_SELECT;

            P1Cursor.GlobalPosition = characterButtons[P1Selected].GlobalPosition;
            P2Cursor.GlobalPosition = characterButtons[P2Selected].GlobalPosition;

            UpdateMenuVisuals();
        }

        private void SelectCharacterP1()
        {
            if (P1Up)
            {
                P1Selected -= charactersContainer.Columns;
                if (P1Selected < 0) P1Selected += charactersContainer.Columns;
            }
            if (P1Down)
            {
                P1Selected += charactersContainer.Columns;
                if (P1Selected > fightersList.elements.Length) 
                    P1Selected -= charactersContainer.Columns;
            }
            if (P1Left)
            {
                
                if (P1Selected % charactersContainer.Columns > 0)
                    P1Selected--;
            }
            if (P1Right)
            {
                if (P1Selected % charactersContainer.Columns < charactersContainer.Columns - 1)
                    P1Selected++;
            }
            if (P1Confirm)
            {
                if (P1Selected >= fightersList.elements.Length)
                    P1Selected = randomSelection.Next(0, fightersList.elements.Length);
                
                P1State = Global.CharacterSelectState.SELECTING_COLOR;
                CallColorSelectP1();
            }
            if (P1Return)
            {
                ReturnToPrevious();   
            }
        }

        private void CallColorSelectP1()
        {
            if (P1ColorSelect == null || fightersList.elements[P1Selected].ColorPalettes == null || fightersList.elements[P1Selected].ColorPalettes.Length <= 1)
            {
                P1State = Global.CharacterSelectState.DONE;
                if (SinglePlayerSelection) P2State = Global.CharacterSelectState.DONE;
                return;
            }

            P1ColorSelect.Activate(fightersList.elements[P1Selected]);
        }

        private void SelectColorP1()
        {
            if (P1Up)
            {
                P1ColorSelected--;
                if (P1ColorSelected < 0) P1ColorSelected = 0;
                P1ColorSelect.SelectElement(P1ColorSelected);
            }
            if (P1Down)
            {
                P1ColorSelected++;
                if (P1ColorSelected >= fightersList.elements[P1Selected].ColorPalettes.Length)
                    P1ColorSelected = fightersList.elements[P1Selected].ColorPalettes.Length - 1;
                P1ColorSelect.SelectElement(P1ColorSelected);
            }
            if (P1Confirm)
            {

                if (!P2Finished) isPlayer1SelectingStage = true;
                P1State = Global.CharacterSelectState.DONE;
                if (SinglePlayerSelection) P2State = Global.CharacterSelectState.DONE;
                P1ColorSelect.Deactivate();
            }
            if (P1Return)
            {

                P1State = Global.CharacterSelectState.SELECTING_CHARACTER;
                P1ColorSelect.Deactivate();
            }
        }

        private void SelectCharacterP2()
        {
            if (PlayerSelection > 0 && !P1Finished) return;
            if (P2Up)
            {
                P2Selected -= charactersContainer.Columns;
                if (P2Selected < 0) P2Selected += charactersContainer.Columns;
            }
            if (P2Down)
            {
                P2Selected += charactersContainer.Columns;
                if (P2Selected > fightersList.elements.Length)
                    P2Selected -= charactersContainer.Columns;
            }
            if (P2Left)
            {
                if (P2Selected % charactersContainer.Columns > 0)
                    P2Selected--;
            }
            if (P2Right)
            {
                if (P2Selected % charactersContainer.Columns < charactersContainer.Columns - 1)
                    P2Selected++;
            }
            if (P2Confirm)
            {
                if (P2Selected >= fightersList.elements.Length)
                    P2Selected = randomSelection.Next(0, fightersList.elements.Length);

                P2State = Global.CharacterSelectState.SELECTING_COLOR;
                CallColorSelectP2();
            }
            if (P2Return)
            {
                ReturnToPrevious();
            }
        }

        private void CallColorSelectP2()
        {
            if (P2ColorSelect == null || fightersList.elements[P2Selected].ColorPalettes == null || fightersList.elements[P2Selected].ColorPalettes.Length <= 1)
            {
                P2State = Global.CharacterSelectState.DONE;
                return;
            }

            P2ColorSelect.Activate(fightersList.elements[P2Selected]);
        }

        private void SelectColorP2()
        {
            if (P2Up)
            {
                P2ColorSelected--;
                if (P2ColorSelected < 0) P2ColorSelected = 0;
                P2ColorSelect.SelectElement(P2ColorSelected);
            }
            if (P2Down)
            {
                P2ColorSelected++;
                if (P2ColorSelected >= fightersList.elements[P2Selected].ColorPalettes.Length)
                    P2ColorSelected = fightersList.elements[P2Selected].ColorPalettes.Length - 1;
                P2ColorSelect.SelectElement(P2ColorSelected);
            }
            if (P2Confirm)
            {

                if (!P1Finished) isPlayer1SelectingStage = false;
                P2State = Global.CharacterSelectState.DONE;
                P2ColorSelect.Deactivate();
            }
            if (P2Return)
            {

                P2State = Global.CharacterSelectState.SELECTING_CHARACTER;
                P2ColorSelect.Deactivate();
            }
        }

        private void ReturnToPrevious()
        {
            if (PlayerSelection > 0 && P1Finished)
            {
                P1State = Global.CharacterSelectState.SELECTING_CHARACTER;
                P2State = Global.CharacterSelectState.SELECTING_CHARACTER;
                return;
            }
            if (returnTo == "")
            {
                Visible = false;
                return;
            }

            LoadingScreenManager.Instance.LoadScene(returnTo);
        }

        private void SelectStage()
        {
            if (!AllowSelectStage) { MatchSetup(); return; }
            bool Up, Down, Left, Right, Confirm, Return;
            if (isPlayer1SelectingStage)
            {
                Up = P1Up;
                Down = P1Down;
                Left = P1Left;
                Right = P1Right;
                Confirm = P1Confirm;
                Return = P1Return;
            }
            else
            {
                Up = P2Up;
                Down = P2Down;
                Left = P2Left;
                Right = P2Right;
                Confirm = P2Confirm;
                Return = P2Return;
            }
                
            if (Left)
            {
                StageSelected--;
                if (StageSelected < -2) StageSelected = -2;
            }
            if (Right)
            {
                StageSelected++;
                if (StageSelected >= stagesList.elements.Length) 
                    StageSelected = stagesList.elements.Length - 1;
            }
            if (Up)
            {
                BGMSelected--;
                if (BGMSelected < -2) BGMSelected = -2;
            }
            if (Down)
            {
                BGMSelected++;
                if (BGMSelected >= songsList.elements.Length) 
                    BGMSelected = songsList.elements.Length - 1;
            }
            if (Confirm)
            {
                if (StageSelected == -2)
                    StageSelected = fightersList.elements[P2Selected].Profile.AutoStage;
                else if (StageSelected == -1)
                    StageSelected = randomSelection.Next(0, stagesList.elements.Length);
                
                if (BGMSelected == -2)
                    BGMSelected = fightersList.elements[P2Selected].Profile.AutoBGM;
                else if (BGMSelected == -1)
                    BGMSelected = randomSelection.Next(0, songsList.elements.Length);
                
                AllSet = true;
                MatchSetup();
            }
            if (Return)
            {
                P1State = Global.CharacterSelectState.SELECTING_CHARACTER;
                P2State = Global.CharacterSelectState.SELECTING_CHARACTER;
            }
        }

        private void UpdateMenuVisuals()
        {
            bool canShowP2 = SinglePlayerSelection || (PlayerSelection > 0 && !P1Finished);
            P2SelectedRender.Visible = !canShowP2;
            P2SelectedName.Visible = !canShowP2;
            P2Cursor.Visible = !canShowP2;

            if (P1Selected < fightersList.elements.Length)
            {
                P1SelectedRender.Texture = fightersList.elements[P1Selected].Profile.Render;
                P1SelectedName.Text = fightersList.elements[P1Selected].Profile.FighterName;
            }
            else
            {
                P1SelectedRender.Texture = randomCharRender;
                P1SelectedName.Text = "Random";
            }
            if (P2Selected < fightersList.elements.Length)
            {
                P2SelectedRender.Texture = fightersList.elements[P2Selected].Profile.Render;
                P2SelectedName.Text = fightersList.elements[P2Selected].Profile.FighterName;
            }
            else
            {
                P2SelectedRender.Texture = randomCharRender;
                P2SelectedName.Text = "Random";
            }

            P1SelectingStage.Visible = isPlayer1SelectingStage;
            P2SelectingStage.Visible = !isPlayer1SelectingStage;

            StageCursor.GlobalPosition = stageButtons[StageSelected + 2].GlobalPosition;

            if (SelectionMode == Global.CharacterSelectMode.STAGE_SELECT)
            {
                if (StageSelected == -2)//Auto
                {
                    StageSelectedRender.Texture = autoStageThumbnail;
                    StageSelectedName.Text = "Auto";
                }
                else if (StageSelected == -1)//Random
                {
                    StageSelectedRender.Texture = randomStageThumbnail;
                    StageSelectedName.Text = "Random";
                }
                else if (StageSelected >= 0)
                {
                    StageSelectedRender.Texture = stagesList.elements[StageSelected].Thumbnail;
                    StageSelectedName.Text = stagesList.elements[StageSelected].Name;
                }
            }
            else
            {
                StageSelectedRender.Texture = null;
            }
            
            if (BGMSelected == -2) // Auto
                SongSelectedName.Text = "Auto";
            else if (BGMSelected == -1) // Random
                SongSelectedName.Text = "Random";
            else if (BGMSelected >= 0)
                SongSelectedName.Text = songsList.elements[BGMSelected].SongName;
        }

        void MatchSetup()
        {
            // Player 1 settings
            Match.P1SelectedCharacter = P1Selected;
            Match.P1SelectedColor = P1ColorSelected;
            // Player 2 settings
            Match.P2SelectedCharacter = P2Selected;
            Match.P2SelectedColor = P2ColorSelected;
            //Other settings
            Match.SelectedStage = StageSelected;
            Match.SelectedBGM = BGMSelected;

            if (destination == null)
            {
                Visible = false;
                return;
            }
            LoadingScreenManager.Instance.LoadMatch(destination, fightersList.elements[P1Selected].Profile, fightersList.elements[P2Selected].Profile);
        }
    }
}
