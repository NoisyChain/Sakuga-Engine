using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class BounceXAction : FrameDataAction
    {
        [Export] private bool CustomIntensity;
        [Export] private int Intensity;

        public override void Execute(ref SakugaActor Actor)
        {
            if (!Actor.CanBounce()) return;

            if (CustomIntensity) Actor.BounceXIntensity = Intensity;

            Actor.Body.BounceX(Actor.BounceXIntensity);
            Actor.Bounce.Stop();
        }
    }
}
