using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterStance : Resource
    {
        [ExportCategory("Settings")]
        [Export] public bool IsDamagePersistent;
        [Export] public bool IsRoundPersistent;
        [Export] public int NeutralState = 0;
        [Export] public MoveSettings[] Moves;
        [Export] public int[] HitReactions;

        [ExportCategory("Blocking")]
        [Export] public int GroundBlockInitialState = -1;
        [Export] public int CrouchBlockInitialState = -1;
        [Export] public int AirBlockInitialState = -1;

        //[ExportCategory("Recovery")]
    }
}
