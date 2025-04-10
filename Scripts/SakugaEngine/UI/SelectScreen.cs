using Godot;
using SakugaEngine;
using SakugaEngine.Resources;
using System;

public partial class SelectScreen : Node
{
    [Export] private FighterList fightersList;
    [Export] private StageList stagesList;
    [Export] private TextureRect P1SelectedRender;
    [Export] private TextureRect P2SelectedRender;
    [Export] private Label P1SelectedName;
    [Export] private Label P2SelectedName;
    [Export] private TextureRect StageSelectedRender;
    [Export] private Label StageSelectedName;

    [Export] private PackedScene charactersButtonElement;
    [Export] private GridContainer charactersContainer;
    [Export] private PackedScene stagesButtonElement;
    [Export] private HBoxContainer stagesContainer;

    [Export] private int P1Selected = 0;
    [Export] private int P2Selected = 1;
    [Export] private int StageSelected = 0;

    private bool P1Finished;
    private bool P2Finished;
    private bool isPlayer1SelectingStage = true;
    private bool AllSet = false;

    [Export] private TextureRect P1Cursor;
    [Export] private TextureRect P2Cursor;
    [Export] private Control P1SelectingStage;
    [Export] private Control P2SelectingStage;
    [Export] private Control StageCursor;
    [Export(PropertyHint.Enum, "Character_Select,Stage_Select")] private byte selectionMode;
    [Export] private Control CharacterSelectMode;
    [Export] private Control StageSelectMode;

    private TextureRect[] characterButtons;
    private TextureRect[] stageButtons;

    public override void _Ready()
    {
        base._Ready();

        characterButtons = new TextureRect[fightersList.elements.Length];
        for (int i = 0; i < fightersList.elements.Length; i++)
        {
            TextureRect temp = charactersButtonElement.Instantiate() as TextureRect;
            temp.Name = fightersList.elements[i].Profile.ShortName + "_Portrait";
            temp.Texture = fightersList.elements[i].Profile.Portrait;
            characterButtons[i] = temp;
            charactersContainer.AddChild(temp);
        }

        stageButtons = new TextureRect[stagesList.elements.Length];
        for (int i = 0; i < stagesList.elements.Length; i++)
        {
            TextureRect temp = stagesButtonElement.Instantiate() as TextureRect;
            temp.Name = stagesList.elements[i].Name + "_Portrait";
            temp.Texture = stagesList.elements[i].Thumbnail;
            stageButtons[i] = temp;
            stagesContainer.AddChild(temp);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (AllSet) return;
        base._PhysicsProcess(delta);
        if (!P1Finished)
        {
            if (Input.IsActionJustPressed("k1_up"))
            {
                P1Selected -= charactersContainer.Columns;
                if (P1Selected < 0) P1Selected = 0;
            }
            if (Input.IsActionJustPressed("k1_down"))
            {
                P1Selected += charactersContainer.Columns;
                if (P1Selected >= fightersList.elements.Length) 
                    P1Selected = fightersList.elements.Length - 1;
            }
            if (Input.IsActionJustPressed("k1_left"))
            {
                P1Selected--;
                if (P1Selected < 0) P1Selected = 0;
            }
            if (Input.IsActionJustPressed("k1_right"))
            {
                P1Selected++;
                if (P1Selected >= fightersList.elements.Length) 
                    P1Selected = fightersList.elements.Length - 1;
            }
            if (Input.IsActionJustPressed("k1_face_a"))
            {
                if (!P2Finished) isPlayer1SelectingStage = true;
                P1Finished = true;
            }
        }
        else
        {
            if (isPlayer1SelectingStage)
            {
                if (Input.IsActionJustPressed("k1_left"))
                {
                    StageSelected--;
                    if (StageSelected < 0) StageSelected = 0;
                }
                if (Input.IsActionJustPressed("k1_right"))
                {
                    StageSelected++;
                    if (StageSelected >= stagesList.elements.Length) 
                        StageSelected = stagesList.elements.Length - 1;
                }
                if (Input.IsActionJustPressed("k1_face_a"))
                {
                    AllSet = true;
                    MatchSetup();
                    GetTree().ChangeSceneToFile("res://Scenes/TestScene.tscn");
                }
                
                if (Input.IsActionJustPressed("k1_face_b"))
                {
                    P1Finished = false;
                    P2Finished = false;
                }
            }
        }

        if (!P2Finished)
        {
            if (Input.IsActionJustPressed("k2_up"))
            {
                P2Selected -= charactersContainer.Columns;
                if (P2Selected < 0) P2Selected = 0;
            }
            if (Input.IsActionJustPressed("k2_down"))
            {
                P2Selected += charactersContainer.Columns;
                if (P2Selected >= fightersList.elements.Length) 
                    P2Selected = fightersList.elements.Length - 1;
            }
            if (Input.IsActionJustPressed("k2_left"))
            {
                P2Selected--;
                if (P2Selected < 0) P2Selected = 0;
            }
            if (Input.IsActionJustPressed("k2_right"))
            {
                P2Selected++;
                if (P2Selected >= fightersList.elements.Length) 
                    P2Selected = fightersList.elements.Length - 1;
            }
            if (Input.IsActionJustPressed("k2_face_a"))
            {
                if (!P1Finished) isPlayer1SelectingStage = false;
                P2Finished = true;
            }
        }
        else
        {
            if (!isPlayer1SelectingStage)
            {
                if (Input.IsActionJustPressed("k2_left"))
                {
                    StageSelected--;
                    if (StageSelected < 0) StageSelected = 0;
                }
                if (Input.IsActionJustPressed("k2_right"))
                {
                    StageSelected++;
                    if (StageSelected >= stagesList.elements.Length) 
                        StageSelected = stagesList.elements.Length - 1;
                }
                if (Input.IsActionJustPressed("k2_face_a"))
                {
                    AllSet = true;
                    MatchSetup();
                    GetTree().ChangeSceneToFile("res://Scenes/TestScene.tscn");
                }
                
                if (Input.IsActionJustPressed("k2_face_b"))
                {
                    P1Finished = false;
                    P2Finished = false;
                }
            }
        }

        selectionMode = P1Finished && P2Finished ? (byte)1 : (byte)0;
        CharacterSelectMode.Visible = selectionMode == 0;
        StageSelectMode.Visible = selectionMode == 1;

        P1Cursor.GlobalPosition = characterButtons[P1Selected].GlobalPosition;
        P2Cursor.GlobalPosition = characterButtons[P2Selected].GlobalPosition;
        
        P1SelectedRender.Texture = fightersList.elements[P1Selected].Profile.Render;
        P1SelectedName.Text = fightersList.elements[P1Selected].Profile.FighterName;
        P2SelectedRender.Texture = fightersList.elements[P2Selected].Profile.Render;
        P2SelectedName.Text = fightersList.elements[P2Selected].Profile.FighterName;

        P1SelectingStage.Visible = isPlayer1SelectingStage;
        P2SelectingStage.Visible = !isPlayer1SelectingStage;

        StageCursor.GlobalPosition = stageButtons[StageSelected].GlobalPosition;

        StageSelectedRender.Texture = selectionMode == 1 ? stagesList.elements[StageSelected].Thumbnail : null;
        StageSelectedName.Text = stagesList.elements[StageSelected].Name;
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
        Global.Match.selectedBGM = 0;
        Global.Match.roundsToWin = 2;
        Global.Match.roundTime = 99;
        Global.Match.selectedMode = Global.SelectedMode.VERSUS;
    }
}
