using Godot;
using System;

public partial class ShowFPS : Label
{
    public override void _Process(double delta)
    {
        Text = Engine.GetFramesPerSecond().ToString() + " fps";
    }
}
