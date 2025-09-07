using Godot;
using SakugaEngine;
using SakugaEngine.Resources;

public partial class SelectScreen : Node
{
    [ExportCategory("Settings")]
    [Export] private FighterList fightersList;
    [Export] private StageList stagesList;
    [Export] private BGMList songsList;
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

    public override void _Ready()
    {
        base._Ready();

        randomSelection = new System.Random();

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
        //Player 1 inputs
        bool P1Up = Input.IsActionJustPressed("k1_up");
        bool P1Down = Input.IsActionJustPressed("k1_down");
        bool P1Left = Input.IsActionJustPressed("k1_left");
        bool P1Right = Input.IsActionJustPressed("k1_right");
        bool P1Confirm = Input.IsActionJustPressed("k1_face_a");
        bool P1Return = Input.IsActionJustPressed("k1_face_b");
        //Player 2 inputs
        bool P2Up = Input.IsActionJustPressed("k2_up");
        bool P2Down = Input.IsActionJustPressed("k2_down");
        bool P2Left = Input.IsActionJustPressed("k2_left");
        bool P2Right = Input.IsActionJustPressed("k2_right");
        bool P2Confirm = Input.IsActionJustPressed("k2_face_a");
        bool P2Return = Input.IsActionJustPressed("k2_face_b");

        switch (selectionMode)
        {
            case 0:
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
                //Player 2 character selection
                if (!P2Finished)
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
        Global.Match.Player1 = new MatchPlayerSettings()
        {
            selectedCharacter = P1Selected,
            selectedColor = 0,
            selectedDevice = 0
        };

        Global.Match.Player2 = new MatchPlayerSettings()
        {
            selectedCharacter = P2Selected,
            selectedColor = 0,
            selectedDevice = 1
        };
        
        Global.Match.selectedStage = StageSelected;
        Global.Match.selectedBGM = BGMSelected;
        Global.Match.roundsToWin = 2;
        Global.Match.roundTime = 99;
        Global.Match.botDifficulty = Global.BotDifficulty.MEDIUM;
        Global.Match.selectedMode = Global.SelectedMode.VERSUS;
    }
}
