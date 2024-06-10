using Godot;
using System;

namespace SakugaEngine.Resources
{
    public partial class SoundsList : Resource
    {
        [Export] public AudioStream[] sounds;
    }
}
