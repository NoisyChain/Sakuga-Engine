using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SetArmorAction : FrameDataAction
    {
        [Export] private int Value;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Value == 0) return;
            if (Actor.Parameters == null) return;
            if (Actor.Parameters.SuperArmor == null) return;

            Actor.Parameters.SuperArmor.SetArmor(Value);
        }
    }
}
