using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SetAttackAction : FrameDataAction
    {
        [Export] private bool Reset;
        [Export] private int Value;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Parameters == null) return;
            if (Actor.Parameters.Prorations == null) return;

            if (Reset) Actor.Parameters.Prorations.CurrentAttack = Actor.Parameters.Prorations.BaseAttack;
            else Actor.Parameters.Prorations.CurrentAttack = (ushort)Value;
        }
    }
}
