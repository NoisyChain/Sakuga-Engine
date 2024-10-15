using Godot;
using System.Text;
using PleaseResync;

public partial class ChecksumCalculator : Control
{
	[Export] private LineEdit TextToConvert;
	[Export] private LineEdit ChecksumField;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_button_pressed()
	{
		byte[] seedArray = Encoding.ASCII.GetBytes(TextToConvert.Text);
		ChecksumField.Text = Platform.GetChecksum(seedArray).ToString();
	}
}
