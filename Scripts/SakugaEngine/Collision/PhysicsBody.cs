using Godot;
using System;

public partial class PhysicsBody : Node3D
{
    [Export] public bool IsStatic;
    [Export] public bool StayOnBounds;
    public uint ID;
    public Collider Pushbox = new Collider();
    public Vector2I FixedPosition;
    public Vector2I FixedVelocity;
    public int PlayerSide;
    public bool IsOnGround => FixedPosition.Y <= 0;
    public bool IsOnLeftWall => FixedPosition.X <= -Global.WallLimit;
    public bool IsOnRightWall => FixedPosition.X >= Global.WallLimit;
    public bool IsOnWall => IsOnLeftWall || IsOnRightWall;

    public override void _Process(double delta)
    {
        Position = Global.ToScaledVector3(FixedPosition);
    }
    public void SetID(uint newID)
    {
        ID = newID;
    }
    public void SetVelocity(Vector2I newVelocity)
    {
        FixedVelocity = newVelocity;
    }
    public void SetLateralVelocity(int newVelocity)
    {
        FixedVelocity.X = newVelocity;
    }
    public void SetVerticalVelocity(int newVelocity)
    {
        FixedVelocity.Y = newVelocity;
    }
    public void AddGravity(int gravity)
    {
        FixedVelocity.Y -= gravity / Global.Delta;
    }
    public void MoveTo(Vector2I destination)
    {
        FixedPosition = destination;
        Pushbox.UpdateCollider(FixedPosition + new Vector2I(0, 6000), new Vector2I(6000, 12000));
    }
    public void Resolve(Vector2I depth)
    {
        FixedPosition -= depth;
        Pushbox.UpdateCollider(FixedPosition + new Vector2I(0, 6000), new Vector2I(6000, 12000));
    }
    public void Move()
    {
        FixedPosition += FixedVelocity / Global.Delta;
        Pushbox.UpdateCollider(FixedPosition + new Vector2I(0, 6000), new Vector2I(6000, 12000));

        if (StayOnBounds)
        {
            FixedPosition.X = Mathf.Clamp(FixedPosition.X, -Global.WallLimit, Global.WallLimit);
            FixedPosition.Y = Mathf.Clamp(FixedPosition.Y, 0, Global.CeilingLimit);
        }
    }
}