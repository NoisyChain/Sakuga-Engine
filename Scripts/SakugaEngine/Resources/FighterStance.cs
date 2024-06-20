using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterStance : Resource
    {
        [Export] public int NeutralState = 0;
        [Export] public MoveSettings[] Moves;
        [Export] public int[] HitReactions;

    }
}
