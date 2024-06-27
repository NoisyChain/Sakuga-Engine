using Godot;
using System;

namespace SakugaEngine
{
    public partial class FighterCamera : Camera3D
    {
        //private Listener audioListener;

        [Export] public bool isCinematic;
        [Export] public Vector2 minBounds = new Vector2(-5.5f, 1.25f), maxBounds = new Vector2(5.50f, 10f);
        //public int limitPlayersDistance = 600;
        [Export] public Vector2 minOffset = new Vector2(-4f, 1.2f), maxOffset = new Vector2(-5f, 1.55f);
        [Export] public float minSmoothDistance = 4;
        [Export] public float minDistance = 4f, maxDistance = 5.5f;
        [Export] public float boundsAdditionalNear = 2.3f, boundsAdditionalFar = 2.95f;

        private Camera3D charCam;

        const float DELTA = 1f / Global.TicksPerSecond;

        public override void _Ready()
        {
            charCam = GetNode<Camera3D>("../CanvasLayer/ViewportContainer/Viewport_Foreground/CharacterCamera");
            //audioListener = GetNode<Listener>("Listener");
        }

        public void UpdateCamera(Node3D player1, Node3D player2)
        {
            if (player1 == null || player2 == null) return;

            Vector3 _p1Position = player1.GlobalPosition;
            Vector3 _p2Position = player2.GlobalPosition;

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
                Mathf.Lerp(Position.X, newCamPosition.X, 10f * DELTA),
                Mathf.Lerp(Position.Y, newCamPosition.Y, 10f * DELTA),
                0);
            Position = new Vector3(
                Mathf.Clamp(Position.X, minBounds.X + BoundsAdd, maxBounds.X - BoundsAdd),
                Mathf.Clamp(Position.Y, minBounds.Y, maxBounds.Y), 
                -FinalZOffset);
            
            charCam.GlobalTransform = GlobalTransform;
            charCam.Fov = Fov;

            //audioListener.GlobalTranslation = new Vector3(Position.X, Position.Y, 0);
        }
    }
}
