using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class SetDefenseAction : FrameDataAction
    {
        [Export] private bool Reset;
        [Export] private int Value;

        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.Parameters == null) return;
            if (Actor.Parameters.Prorations == null) return;

            if (Reset) Actor.Parameters.Prorations.CurrentDefense = Actor.Parameters.Prorations.BaseDefense;
            else Actor.Parameters.Prorations.CurrentDefense = (ushort)Value;
        }
    }
}