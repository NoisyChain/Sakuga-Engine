using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterStance : Resource
    {
        [ExportCategory("Settings")]
        [Export] public bool IsPersistent;
        [Export] public int NeutralState = 0;
        [Export] public MoveSettings[] Moves;
        [Export] public int[] HitReactions;

        [ExportCategory("Blocking")]
        public int GroundBlockInitialState = -1;
        public int CrouchBlockInitialState = -1;
        public int AirBlockInitialState = -1;

        //[ExportCategory("Recovery")]
    }
}
