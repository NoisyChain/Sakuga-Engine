using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class BounceYAction : FrameDataAction
    {
        [Export] private bool CustomIntensity;
        [Export] private int Intensity;

        public override void Execute(ref SakugaActor Actor)
        {
            if (!Actor.CanBounce()) return;

            if (CustomIntensity) Actor.BounceYIntensity = Intensity;

            Actor.Body.BounceY(Actor.BounceYIntensity);
            Actor.Bounce.Stop();
        }
    }
}
