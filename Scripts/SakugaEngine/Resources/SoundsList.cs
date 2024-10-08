using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SoundsList : Resource
    {
        [Export] public AudioStream[] Sounds;
    }
}
