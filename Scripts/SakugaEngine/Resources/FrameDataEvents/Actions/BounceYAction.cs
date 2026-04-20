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
            if (CustomIntensity) Actor.BounceYIntensity = Intensity;

            Actor.Body.BounceY(Actor.BounceYIntensity);
            Actor.BounceYIntensity = 0;
            Actor.Bounce.Stop();
        }
    }
}
