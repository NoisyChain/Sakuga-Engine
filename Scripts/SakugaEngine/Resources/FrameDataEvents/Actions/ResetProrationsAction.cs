using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class ResetProrationsAction : FrameDataAction
	{
		public override void Execute(ref SakugaActor Actor)
		{
			if (Actor.Parameters == null) return;
			if (Actor.Parameters.Prorations == null) return;

			Actor.Parameters.Prorations.ResetProrations();
		}
	}
}