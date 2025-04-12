using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class BGMElement : Resource
    {
        [Export] public string SongName;
        [Export] public AudioStream clip;
    }
}
