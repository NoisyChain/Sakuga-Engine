using Godot;
using SakugaEngine.Resources;
using System;

namespace SakugaEngine.UI
{
    public partial class MainMenu : Control
    {
        [Export] private Button firstButton;
        [Export] public MatchSettings matchSettings;

        [ExportCategory("InputSelect")]
        [Export] private Control InputSelectMenu;
        [Export] private Control DeviceWidgetKeyboard1;
        [Export] private Control DeviceWidgetKeyboard2;
        [Export] private Control DeviceWidgetController1;
        [Export] private Control DeviceWidgetController2;
        [Export] private Control PivotP1;
        [Export] private Control PivotP2;

        Vector2 k1Pos;
        Vector2 k2Pos;
        Vector2 j1Pos;
        Vector2 j2Pos;

        public override void _Ready()
        {
            base._Ready();
            firstButton.GrabFocus();
            k1Pos = DeviceWidgetKeyboard1.Position;
            k2Pos = DeviceWidgetKeyboard2.Position;
            j1Pos = DeviceWidgetController1.Position;
            j2Pos = DeviceWidgetController2.Position;

            matchSettings.P1SelectedDevice = -1;
            matchSettings.P2SelectedDevice = -1;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (!InputSelectMenu.Visible) return;

            if (Input.IsActionJustPressed("k1_left"))
                if (matchSettings.P2SelectedDevice == 0) matchSettings.P2SelectedDevice = -1;
                else if (matchSettings.P1SelectedDevice == -1) matchSettings.P1SelectedDevice = 0;
            if (Input.IsActionJustPressed("k1_right"))
                if (matchSettings.P1SelectedDevice == 0) matchSettings.P1SelectedDevice = -1;
                else if (matchSettings.P2SelectedDevice == -1) matchSettings.P2SelectedDevice = 0;
            
            if (matchSettings.P2SelectedDevice == 0) DeviceWidgetKeyboard1.Position = PivotP2.Position;
            else if (matchSettings.P1SelectedDevice == 0) DeviceWidgetKeyboard1.Position = PivotP1.Position;
            else DeviceWidgetKeyboard1.Position = k1Pos;

            if (Input.IsActionJustPressed("k2_left"))
                if (matchSettings.P2SelectedDevice == 1) matchSettings.P2SelectedDevice = -1;
                else if (matchSettings.P1SelectedDevice == -1) matchSettings.P1SelectedDevice = 1;
            if (Input.IsActionJustPressed("k2_right"))
                if (matchSettings.P1SelectedDevice == 1) matchSettings.P1SelectedDevice = -1;
                else if (matchSettings.P2SelectedDevice == -1) matchSettings.P2SelectedDevice = 1;
            
            if (matchSettings.P2SelectedDevice == 1) DeviceWidgetKeyboard2.Position = PivotP2.Position;
            else if (matchSettings.P1SelectedDevice == 1) DeviceWidgetKeyboard2.Position = PivotP1.Position;
            else DeviceWidgetKeyboard2.Position = k2Pos;

            if (Input.IsActionJustPressed("j1_left"))
                if (matchSettings.P2SelectedDevice == 2) matchSettings.P2SelectedDevice = -1;
                else if (matchSettings.P1SelectedDevice == -1) matchSettings.P1SelectedDevice = 2;
            if (Input.IsActionJustPressed("j1_right"))
                if (matchSettings.P1SelectedDevice == 2) matchSettings.P1SelectedDevice = -1;
                else if (matchSettings.P2SelectedDevice == -1) matchSettings.P2SelectedDevice = 2;
            
            if (matchSettings.P2SelectedDevice == 2) DeviceWidgetController1.Position = PivotP2.Position;
            else if (matchSettings.P1SelectedDevice == 2) DeviceWidgetController1.Position = PivotP1.Position;
            else DeviceWidgetController1.Position = j1Pos;

            if (Input.IsActionJustPressed("j2_left"))
                if (matchSettings.P2SelectedDevice == 3) matchSettings.P2SelectedDevice = -1;
                else if (matchSettings.P1SelectedDevice == -1) matchSettings.P1SelectedDevice = 3;
            if (Input.IsActionJustPressed("j2_right"))
                if (matchSettings.P1SelectedDevice == 3) matchSettings.P1SelectedDevice = -1;
                else if (matchSettings.P2SelectedDevice == -1) matchSettings.P2SelectedDevice = 3;
            
            if (matchSettings.P2SelectedDevice == 3) DeviceWidgetController2.Position = PivotP2.Position;
            else if (matchSettings.P1SelectedDevice == 3) DeviceWidgetController2.Position = PivotP1.Position;
            else DeviceWidgetController2.Position = j2Pos;

            bool key1Confirm = (matchSettings.P1SelectedDevice == 0 || matchSettings.P2SelectedDevice == 0) && Input.IsActionJustPressed("k1_accept");
            bool key2Confirm = (matchSettings.P1SelectedDevice == 1 || matchSettings.P2SelectedDevice == 1) && Input.IsActionJustPressed("k2_accept");
            bool joy1Confirm = (matchSettings.P1SelectedDevice == 2 || matchSettings.P2SelectedDevice == 2) && Input.IsActionJustPressed("j1_accept");
            bool joy2Confirm = (matchSettings.P1SelectedDevice == 3 || matchSettings.P2SelectedDevice == 3) && Input.IsActionJustPressed("j2_accept");

            if (Input.IsActionJustPressed("general_return")) InputSelectMenu.Visible = false;

            if (key1Confirm || key2Confirm || joy1Confirm || joy2Confirm) GetTree().ChangeSceneToFile("res://Scenes/SelectScreen.tscn");

        }


        public void _OnOnlineButtonPressed()
        {
            matchSettings.SelectedMode = 2;
            matchSettings.P1SelectedDevice = 10;
            matchSettings.P2SelectedDevice = -1;
            GetTree().ChangeSceneToFile("res://Scenes/Lobby.tscn");
        }

        public void _OnArcadeButtonPressed()
        {
            matchSettings.SelectedMode = 0;
            matchSettings.P1SelectedDevice = 10;
            matchSettings.P2SelectedDevice = -1;
            GetTree().ChangeSceneToFile("res://Scenes/SelectScreen.tscn");
        }

        public void _OnVersusButtonPressed()
        {
            matchSettings.SelectedMode = 1;
            InputSelectMenu.Visible = true;
            //GetTree().ChangeSceneToFile("res://Scenes/SelectScreen.tscn");
        }

        public void _OnTrainingButtonPressed()
        {
            matchSettings.SelectedMode = 3;
            matchSettings.P1SelectedDevice = 10;
            matchSettings.P2SelectedDevice = -1;
            GetTree().ChangeSceneToFile("res://Scenes/SelectScreen.tscn");
        }

        public void _OnReplayButtonPressed()
        {
            matchSettings.SelectedMode = 4;
            matchSettings.P1SelectedDevice = -1;
            matchSettings.P2SelectedDevice = -1;
        }

        public void _OnQuitButtonPressed()
        {
            GetTree().Quit();
        }
    }
}
