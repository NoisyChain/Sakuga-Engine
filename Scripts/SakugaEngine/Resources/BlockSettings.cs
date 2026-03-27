using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class BlockSettings : Resource
    {
        [Export] public StanceSelect ReferenceStance;
        [Export] public BlockType BlockType;
        [Export] public MotionInputs InputToCheck;
        [Export] public int EnterState;
        [Export] public int StunState;
        [Export] public int ExitState;
        [Export] public int GuardCrushState;
    }
}