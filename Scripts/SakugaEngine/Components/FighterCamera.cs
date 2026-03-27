using Godot;
using SakugaEngine.Global;

namespace SakugaEngine
{
    public partial class FighterCamera : Camera3D
    {
        private AudioListener3D Listener;
        [Export] public CameraFocus CurrentFocus = CameraFocus.SELF;
        [Export] public bool isCinematic;
        [Export] public Vector2 minBounds = new Vector2(-5.5f, 1.25f), maxBounds = new Vector2(5.50f, 10f);
        [Export] public Vector2 minOffset = new Vector2(-4f, 1.2f), maxOffset = new Vector2(-5f, 1.55f);
        [Export] public float minSmoothDistance = 4;
        [Export] public float minDistance = 4f, maxDistance = 5.5f;
        [Export] public float boundsAdditionalNear = 2.3f, boundsAdditionalFar = 2.95f;

        private Camera3D charCam;

        const float DELTA = 10f / GlobalVariables.TicksPerSecond;

        public override void _Ready()
        {
            charCam = GetNode<Camera3D>("../CanvasLayer/ViewportContainer/Viewport_Foreground/CharacterCamera");
            //audioListener = GetNode<Listener>("Listener");
        }

        public void UpdateCamera(SakugaActor player1, SakugaActor player2)
        {
            if (player1 == null || player2 == null) return;

            Vector3 _p1Position = GlobalFunctions.ToScaledVector3(player1.Body.FixedPosition);
            Vector3 _p2Position = GlobalFunctions.ToScaledVector3(player2.Body.FixedPosition);

            bool canSmooth = Mathf.Abs(_p2Position.X - _p1Position.X) > minSmoothDistance;

            float playerDistance = Mathf.Clamp(Mathf.Abs(_p2Position.X - _p1Position.X), minDistance, maxDistance);
            float pl = (playerDistance - minDistance) / (maxDistance - minDistance);
            float FinalYOffset = Mathf.Lerp(minOffset.Y, maxOffset.Y, pl);
            float FinalZOffset = Mathf.Lerp(minOffset.X, maxOffset.X, pl);

            float finalCamY = 0;
            if (Mathf.Max(_p1Position.Y, _p2Position.Y) >= FinalYOffset)
                finalCamY = Mathf.Max(_p1Position.Y, _p2Position.Y);
            else
                finalCamY = FinalYOffset;

            float actualCenter = (_p1Position.X + _p2Position.X) / 2;
            Vector3 newCamPosition = new Vector3(actualCenter, finalCamY, 0);

            float BoundsAdd = Mathf.Lerp(boundsAdditionalNear, boundsAdditionalFar, pl);
            Position = new Vector3(
                Mathf.Lerp(Position.X, newCamPosition.X, DELTA),
                Mathf.Lerp(Position.Y, newCamPosition.Y, DELTA),
                0);
            Position = new Vector3(
                Mathf.Clamp(Position.X, minBounds.X + BoundsAdd, maxBounds.X - BoundsAdd),
                Mathf.Clamp(Position.Y, minBounds.Y, maxBounds.Y),
                -FinalZOffset);
            
            charCam.GlobalTransform = GlobalTransform;
            var clampedCamPosition = new Vector3(
                Mathf.Clamp(newCamPosition.X, minBounds.X + BoundsAdd, maxBounds.X - BoundsAdd),
                Mathf.Clamp(newCamPosition.Y, minBounds.Y, maxBounds.Y),
                0
            );
            var dist = GlobalPosition.DistanceTo(clampedCamPosition);
            switch (charCam.Projection)
            {
                case ProjectionType.Perspective:
                    charCam.Fov = Fov;
                    break;
                default:
                    //charCam.Position -= new Vector3(0, -0.23f, 0);
                    //charCam.RotationDegrees = new Vector3(-3.5f, 0, 0);
                    charCam.Size = dist * Mathf.Tan(Mathf.DegToRad(Fov * 2) / 2) - Mathf.Lerp(0.2f, 0.4f, pl);
                    break;
            }

            if (Listener != null) Listener.GlobalPosition = new Vector3(Position.X, Position.Y, 0);
        }
    }
}
