using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FrameProperties : Resource
    {
        [Export] public int Frame;
        [Export] public Global.FrameProperties Properties;
    }
}
