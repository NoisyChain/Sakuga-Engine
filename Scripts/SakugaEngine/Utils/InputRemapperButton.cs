using Godot;
using System;

public partial class InputRemapperButton : HBoxContainer
{
	[Export] private string Title;
	[Export] private string Action;
	[Export] private Label TitleLabel;
	[Export] private ColorRect Field;
	[Export] private Label KeyLabel;

    public override void _Ready()
    {
        base._Ready();
		TitleLabel.Text = Title;
    }


	// Called when the node enters the scene tree for the first time.
	public void SetColor(Color color)
	{
		Field.Color = color;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void SetKey(string text)
	{
		KeyLabel.Text = text;
	}

	public string GetAction() => Action;
}
