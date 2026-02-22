using Godot;
using SakugaEngine;
using SakugaEngine.Resources;

public partial class SelectScreen : Node
{
    [ExportCategory("Settings")]
    [Export] private MatchSettings Match;
    [Export] private FighterList fightersList;
    [Export] private StageList stagesList;
    [Export] private BGMList songsList;
    [Export(PropertyHint.Enum, "Versus,Player1,Player2")] private byte PlayerSelection;
    [Export(PropertyHint.Enum, "Character_Select,Stage_Select")] private byte selectionMode;
    [Export] private Control CharacterSelectMode;
    [Export] private Control StageSelectMode;
    [Export] private int P1Selected = 0;
    [Export] private int P2Selected = 1;
    [Export] private int StageSelected = -2;
    [Export] private int BGMSelected = -2;

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
    private bool P1Finished;
    private bool P2Finished;
    private bool isPlayer1SelectingStage = true;
    private bool AllSet = false;
    private TextureRect[] characterButtons;
    private TextureRect[] stageButtons;

    private System.Random randomSelection;

    string p1Prefix;
    string p2Prefix;

    public override void _Ready()
    {
        base._Ready();

        randomSelection = new System.Random();

        if (Match.P1SelectedDevice > -1 && Match.P2SelectedDevice > -1) PlayerSelection = 0;
        else if (Match.P2SelectedDevice == -1) PlayerSelection = 1;
        else if (Match.P1SelectedDevice == -1) PlayerSelection = 2;

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
        if (AllSet) return;
        base._PhysicsProcess(delta);

        string p1SelectPrefix = PlayerSelection != 2 ? p1Prefix : p2Prefix;
        string p2SelectPrefix = PlayerSelection != 1 ? p2Prefix : p1Prefix;
        
        //Player 1 inputs
        bool P1Up = Input.IsActionJustPressed(p1SelectPrefix + "_up");
        bool P1Down = Input.IsActionJustPressed(p1SelectPrefix + "_down");
        bool P1Left = Input.IsActionJustPressed(p1SelectPrefix + "_left");
        bool P1Right = Input.IsActionJustPressed(p1SelectPrefix + "_right");
        bool P1Confirm = Input.IsActionJustPressed(p1SelectPrefix + "_accept");
        bool P1Return = Input.IsActionJustPressed(p1SelectPrefix + "_return");
        //Player 2 inputs
        bool P2Up = Input.IsActionJustPressed(p2SelectPrefix + "_up");
        bool P2Down = Input.IsActionJustPressed(p2SelectPrefix + "_down");
        bool P2Left = Input.IsActionJustPressed(p2SelectPrefix + "_left");
        bool P2Right = Input.IsActionJustPressed(p2SelectPrefix + "_right");
        bool P2Confirm = Input.IsActionJustPressed(p2SelectPrefix + "_accept");
        bool P2Return = Input.IsActionJustPressed(p2SelectPrefix + "_return");

        switch (selectionMode)
        {
            case 0:
                //Player 2 character selection
                if (!P2Finished && (PlayerSelection == 0 || P1Finished))
                {
                    if (P2Up)
                    {
                        P2Selected -= charactersContainer.Columns;
                        if (P2Selected < 0) P2Selected = 0;
                    }
                    if (P2Down)
                    {
                        P2Selected += charactersContainer.Columns;
                        if (P2Selected > fightersList.elements.Length) 
                            P2Selected = fightersList.elements.Length;
                    }
                    if (P2Left)
                    {
                        P2Selected--;
                        if (P2Selected < 0) P2Selected = 0;
                    }
                    if (P2Right)
                    {
                        P2Selected++;
                        if (P2Selected > fightersList.elements.Length) 
                            P2Selected = fightersList.elements.Length;
                    }
                    if (P2Confirm)
                    {
                        if (P2Selected >= fightersList.elements.Length)
                            P2Selected = randomSelection.Next(0, fightersList.elements.Length);

                        if (!P1Finished) isPlayer1SelectingStage = false;
                        P2Finished = true;
                    }
                }
                else
                {
                    if (P2Return)
                    {
                        P2Finished = false;
                    }
                }
                //Player 1 character selection
                if (!P1Finished)
                {
                    if (P1Up)
                    {
                        P1Selected -= charactersContainer.Columns;
                        if (P1Selected < 0) P1Selected = 0;
                    }
                    if (P1Down)
                    {
                        P1Selected += charactersContainer.Columns;
                        if (P1Selected > fightersList.elements.Length) 
                            P1Selected = fightersList.elements.Length;
                    }
                    if (P1Left)
                    {
                        P1Selected--;
                        if (P1Selected < 0) P1Selected = 0;
                    }
                    if (P1Right)
                    {
                        P1Selected++;
                        if (P1Selected > fightersList.elements.Length) 
                            P1Selected = fightersList.elements.Length;
                    }
                    if (P1Confirm)
                    {
                        if (P1Selected >= fightersList.elements.Length)
                            P1Selected = randomSelection.Next(0, fightersList.elements.Length);
                        
                        if (!P2Finished) isPlayer1SelectingStage = true;
                        P1Finished = true;
                    }
                }
                else
                {
                    if (P1Return)
                    {
                        P1Finished = false;
                    }
                }
                break;
            case 1:
                if (isPlayer1SelectingStage)
                {
                    if (P1Left)
                    {
                        StageSelected--;
                        if (StageSelected < -2) StageSelected = -2;
                    }
                    if (P1Right)
                    {
                        StageSelected++;
                        if (StageSelected >= stagesList.elements.Length) 
                            StageSelected = stagesList.elements.Length - 1;
                    }
                    if (P1Up)
                    {
                        BGMSelected--;
                        if (BGMSelected < -2) BGMSelected = -2;
                    }
                    if (P1Down)
                    {
                        BGMSelected++;
                        if (BGMSelected >= songsList.elements.Length) 
                            BGMSelected = songsList.elements.Length - 1;
                    }
                    if (P1Confirm)
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
                        GetTree().ChangeSceneToFile("res://Scenes/TestScene.tscn");
                    }
                    if (P1Return)
                    {
                        P1Finished = false;
                        P2Finished = false;
                    }
                }
                else
                {
                    if (P2Left)
                    {
                        StageSelected--;
                        if (StageSelected < -2) StageSelected = -2;
                    }
                    if (P2Right)
                    {
                        StageSelected++;
                        if (StageSelected >= stagesList.elements.Length) 
                            StageSelected = stagesList.elements.Length - 1;
                    }
                    if (P2Up)
                    {
                        BGMSelected--;
                        if (BGMSelected < -2) BGMSelected = -2;
                    }
                    if (P2Down)
                    {
                        BGMSelected++;
                        if (BGMSelected >= songsList.elements.Length) 
                            BGMSelected = songsList.elements.Length - 1;
                    }
                    if (P2Confirm)
                    {
                        if (StageSelected == -2)
                            StageSelected = fightersList.elements[P1Selected].Profile.AutoStage;
                        else if (StageSelected == -1)
                            StageSelected = randomSelection.Next(0, stagesList.elements.Length);
                        
                        if (BGMSelected == -2)
                            BGMSelected = fightersList.elements[P1Selected].Profile.AutoBGM;
                        else if (BGMSelected == -1)
                            BGMSelected = randomSelection.Next(0, songsList.elements.Length);
                        
                        AllSet = true;
                        MatchSetup();
                        GetTree().ChangeSceneToFile("res://Scenes/TestScene.tscn");
                    }
                    if (P2Return)
                    {
                        P1Finished = false;
                        P2Finished = false;
                    }
                }
                break;
        }

        selectionMode = P1Finished && P2Finished ? (byte)1 : (byte)0;
        CharacterSelectMode.Visible = selectionMode == 0;
        StageSelectMode.Visible = selectionMode == 1;

        P1Cursor.GlobalPosition = characterButtons[P1Selected].GlobalPosition;
        P2Cursor.GlobalPosition = characterButtons[P2Selected].GlobalPosition;

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

        if (selectionMode == 1)
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
        
        if (BGMSelected == -2)//Auto
            SongSelectedName.Text = "Auto";
        else if (BGMSelected == -1)//Random
            SongSelectedName.Text = "Random";
        else if (BGMSelected >= 0)
            SongSelectedName.Text = songsList.elements[BGMSelected].SongName;
    }

    void MatchSetup()
    {
        // Player 1 settings
        Match.P1SelectedCharacter = P1Selected;
        Match.P1SelectedColor = 0;
        // Player 2 settings
        Match.P2SelectedCharacter = P2Selected;
        Match.P2SelectedColor = 0;
        //Other settings
        Match.SelectedStage = StageSelected;
        Match.SelectedBGM = BGMSelected;
        //Match.RoundsToWin = 2;
        //Match.TimeLimit = 99;
        //Match.Difficulty = Global.BotDifficulty.MEDIUM;
    }
}
