using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass] [Tool]
	public partial class AttackLevel : Resource
	{
        [Export] public ushort Priority = 0;
        [Export] public int ArmorDamage = 1;
        [Export] public uint Hitstun = 8;
        [Export] public uint Blockstun = 8;
        [Export] public ushort DamageScalingSubtract = 15;
        [Export] public int CornerPushback = -30000;
	}
}
