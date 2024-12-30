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

        [ExportCategory("Recovery")]
        [Export] public int GroundForwardRecoveryState = -1;
        [Export] public int GroundBackwardsRecoveryState = -1;
        [Export] public int AirForwardRecoveryState = -1;
        [Export] public int AirBackwardsRecoveryState = -1;
        [Export] public int OffTheGroundRecoveryState = -1;

        [ExportCategory("Throw Escape")]
        [Export] public int GroundThrowEscapeState = -1;
        [Export] public int AirThrowEscapeState = -1;
    }
}
