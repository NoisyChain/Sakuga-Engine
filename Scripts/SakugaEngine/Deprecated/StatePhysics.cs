using Godot;

namespace SakugaEngine.Resources
{
     [GlobalClass]
    public partial class StatePhysics : Resource
    {
        [Export] public int Frame;
        [Export] public bool UseLateralSpeed;
        [Export] public int LateralSpeed = 0;
        [Export] public bool UseVerticalSpeed;
        [Export] public int VerticalSpeed = 0;
        [Export] public bool UseGravity;
        [Export] public bool ResetGravity;
        [Export] public int Gravity = 0;
        [Export] public bool UseLateralInertia;
        [Export] public bool UseVerticalInertia;
        [Export] public bool UseHorizontalInput;
        [Export] public bool UseVerticalInput;
    }
}
