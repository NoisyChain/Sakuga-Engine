using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CameraShakeAction : FrameDataAction
    {
        [Export] public float Amplitude;
        [Export] public float WaveLength;
        [Export] public int Duration;
        [Export] public bool SideRelative;
        [Export] public Vector2 Direction;

        public override void Execute(ref SakugaActor Actor)
        {
            
        }
    }
}
