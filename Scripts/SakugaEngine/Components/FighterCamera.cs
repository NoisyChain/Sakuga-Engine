using Godot;
using SakugaEngine.Global;

namespace SakugaEngine
{
    public partial class FighterCamera : Camera3D
    {
        private AudioListener3D Listener;
        [Export] public CameraFocus CurrentFocus = CameraFocus.CENTER;
        //[Export] public Vector2 minBounds = new Vector2(-12.0f, 0.0f), maxBounds = new Vector2(12.0f, 20.0f);
        [Export] public Vector3 nearOffset = new Vector3(2.7f, 1.4f, 6.5f), farOffset = new Vector3(7.25f, 3.35f, 16.0f);
        //[Export] public float minSmoothDistance = 4;
        [Export] public float nearDistance = 4.0f, farDistance = 16.0f;
        [Export] public float nearFlatOffset = 0.25f, farFlatOffset = 0.62f;

        private Camera3D charCam;

        const float DELTA = 10f / GlobalVariables.TicksPerSecond;

        float _wallLimit = GlobalFunctions.ToScaledFloat(GlobalVariables.WallLimit);
        float _ceilingLimit = GlobalFunctions.ToScaledFloat(GlobalVariables.CeilingLimit);
        float _groundLimit = 0f;

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

            //bool canSmooth = Mathf.Abs(_p2Position.X - _p1Position.X) > minSmoothDistance;

            float playerDistance = Mathf.Clamp(Mathf.Abs(_p2Position.X - _p1Position.X), nearDistance, farDistance);
            float pl = (playerDistance - nearDistance) / (farDistance - nearDistance);

            float FinalYOffset = nearOffset.Y;
            float FinalZOffset = -nearOffset.Z;
            float BoundsAdd = nearOffset.X;
            float FlatOffset = nearFlatOffset;

            float finalCamY = FinalYOffset;
            float actualCenter = (_p1Position.X + _p2Position.X) / 2;
            switch (CurrentFocus)
            {
                case CameraFocus.CENTER:
                    FinalYOffset = Mathf.Lerp(nearOffset.Y, farOffset.Y, pl);
                    FinalZOffset = Mathf.Lerp(-nearOffset.Z, -farOffset.Z, pl);
                    BoundsAdd = Mathf.Lerp(nearOffset.X, farOffset.X, pl);
                    FlatOffset = Mathf.Lerp(nearFlatOffset, farFlatOffset, pl);

                    //actualCenter = (_p1Position.X + _p2Position.X) / 2;
                    if (Mathf.Max(_p1Position.Y, _p2Position.Y) >= FinalYOffset)
                        finalCamY = Mathf.Max(_p1Position.Y, _p2Position.Y);
                    break;
                case CameraFocus.PLAYER1:
                    //pl = nearDistance;
                    actualCenter = _p1Position.X;
                    if (_p1Position.Y >= FinalYOffset)
                        finalCamY = _p1Position.Y;
                    break;
                case CameraFocus.PLAYER2:
                    //pl = nearDistance;
                    actualCenter = _p2Position.X;
                    if (_p2Position.Y >= FinalYOffset)
                        finalCamY = _p2Position.Y;
                    break;
                default:
                    break;
            }

            Vector3 newCamPosition = new Vector3(actualCenter, finalCamY, 0);
            var clampedCamPosition = new Vector3(
                Mathf.Clamp(newCamPosition.X, -_wallLimit + BoundsAdd, _wallLimit - BoundsAdd),
                Mathf.Clamp(newCamPosition.Y, _groundLimit + FinalYOffset, _ceilingLimit - FinalYOffset),
                0
            );
            Position = new Vector3(
                Mathf.Clamp(Mathf.Lerp(Position.X, newCamPosition.X, DELTA), -_wallLimit + BoundsAdd, _wallLimit - BoundsAdd),
                Mathf.Clamp(Mathf.Lerp(Position.Y, newCamPosition.Y, DELTA), _groundLimit + FinalYOffset, _ceilingLimit - FinalYOffset),
                -FinalZOffset);
            
            charCam.GlobalTransform = GlobalTransform;
            
            var dist = GlobalPosition.DistanceTo(clampedCamPosition);
            switch (Projection)
            {
                case ProjectionType.Perspective:
                    switch (charCam.Projection)
                    {
                        case ProjectionType.Perspective:
                            charCam.Fov = Fov;
                            break;
                        default:
                            charCam.Size = dist * Mathf.Tan(Mathf.DegToRad(Fov * 2) / 2) - FlatOffset;
                            break;
                    }
                    break;
                default:
                    charCam.Projection = Projection;
                    Size = Position.Z;
                    charCam.Size = Size;
                    break;
            }
            
            if (Listener != null) Listener.GlobalPosition = new Vector3(Position.X, Position.Y, 0);
        }
    }
}
