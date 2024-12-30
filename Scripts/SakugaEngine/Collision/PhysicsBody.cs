using Godot;
using System.IO;
using SakugaEngine.Resources;

namespace SakugaEngine.Collision
{
    [GlobalClass]
    public partial class PhysicsBody : Node
    {
        [Export] public bool IsStatic;
        [Export] public bool StayOnBounds;
        [Export] public bool IgnoreWalls;
        [Export] public int FixedAcceleration;
        [Export] public int FixedDeceleration;
        [Export] public int FixedFriction;
        [Export] public int HitboxesLimit = 16;
        [Export] public HitboxSettings[] HitboxPresets;
        public IDamage Parent;
        public uint ID;
        public Collider Pushbox;
        public Collider[] Hitboxes;

        public Vector2I FixedPosition;
        public Vector2I FixedVelocity;
        public bool IsLeftSide;
        public int PlayerSide;
        public bool IsMovable = true;
        public bool HitConfirmed;
        public bool ProximityBlocked;
        public int CurrentHitbox = -1;
        public byte FrameProperties = 0;
        
        public bool IsOnGround => FixedPosition.Y <= 0;
        public bool IsOnLeftWall => FixedPosition.X <= -Global.WallLimit;
        public bool IsOnRightWall => FixedPosition.X >= Global.WallLimit;
        public bool IsFalling => !IsOnGround && FixedVelocity.Y <= 0;
        public bool JustLanded => IsOnGround && FixedVelocity.Y < 0;
        public bool IsOnWall => IsOnLeftWall || IsOnRightWall;
        

        public void Initialize(IDamage owner)
        {
            FixedPosition = Vector2I.Zero;
            FixedVelocity = Vector2I.Zero;
            Parent = owner;
            Hitboxes = new Collider[HitboxesLimit];
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
        public void AddLateralAcceleration(int newVelocity)
        {
            int absVelocity = Mathf.Abs(newVelocity);
            if (newVelocity != 0 && Mathf.Sign(newVelocity) > 0)
            {
                if (FixedVelocity.X < 0)
                    FixedVelocity.X += FixedFriction / Global.TicksPerSecond;
                else if (FixedVelocity.X < absVelocity)
                    FixedVelocity.X += FixedAcceleration / Global.TicksPerSecond;
                    
            }
            else if (newVelocity != 0 && Mathf.Sign(newVelocity) < 0)
            {
                if (FixedVelocity.X > 0)
                    FixedVelocity.X -= FixedFriction / Global.TicksPerSecond;
                else if (FixedVelocity.X > -absVelocity)
                    FixedVelocity.X -= FixedAcceleration / Global.TicksPerSecond;
            }
            else
                FixedVelocity.X -= Mathf.Min(Mathf.Abs(FixedVelocity.X), FixedDeceleration / Global.TicksPerSecond) * Mathf.Sign(FixedVelocity.X);
        }
        public void AddVerticalAcceleration(int newVelocity)
        {
            int absVelocity = Mathf.Abs(newVelocity);
            if (newVelocity != 0 && Mathf.Sign(newVelocity) > 0)
            {
                if (FixedVelocity.Y < 0)
                    FixedVelocity.Y += FixedFriction / Global.TicksPerSecond;
                else if (FixedVelocity.Y < absVelocity)
                    FixedVelocity.Y += FixedAcceleration / Global.TicksPerSecond;
                    
            }
            else if (newVelocity != 0 && Mathf.Sign(newVelocity) < 0)
            {
                if (FixedVelocity.Y > 0)
                    FixedVelocity.Y -= FixedFriction / Global.TicksPerSecond;
                else if (FixedVelocity.Y > -absVelocity)
                    FixedVelocity.Y -= FixedAcceleration / Global.TicksPerSecond;
            }
            else
                FixedVelocity.Y -= Mathf.Min(Mathf.Abs(FixedVelocity.Y), FixedDeceleration / Global.TicksPerSecond) * Mathf.Sign(FixedVelocity.Y);
        }
        public void AddGravity(int gravity)
        {
            FixedVelocity.Y -= gravity / Global.TicksPerSecond;
        }
        public void MoveTo(Vector2I destination)
        {
            if (IsStatic) return;
            //if (!IsMovable) return;
            
            FixedPosition = destination;
            UpdateColliders();
        }
        public void Resolve(Vector2I depth)
        {
            FixedPosition -= depth;
            UpdateColliders();
        }
        public void Move()
        {
            if (IsStatic) return;
            if (!IsMovable) return;

            FixedPosition += FixedVelocity / Global.Delta;
            UpdateColliders();
        }

        public void SetHitbox(int index)
        {
            CurrentHitbox = index;
            UpdateColliders();
        }

        public bool ContainsFrameProperty(byte CompareTo)
        {
            return (FrameProperties & CompareTo) != 0;
        }

        public void UpdateColliders()
        {
            //Lock collider on bounds
            if (StayOnBounds)
            {
                if (!IgnoreWalls)
                    FixedPosition.X = Mathf.Clamp(FixedPosition.X, -Global.WallLimit, Global.WallLimit);
                FixedPosition.Y = Mathf.Clamp(FixedPosition.Y, 0, Global.CeilingLimit);
            }

            if (CurrentHitbox < 0) //If there's no hitbox active, remove all boxes
            {
                Pushbox.UpdateCollider(FixedPosition, Vector2I.Zero);

                for(int i = 0; i < Hitboxes.Length; i++)
                    Hitboxes[i].UpdateCollider(FixedPosition, Vector2I.Zero);
            }
            else //Update hitboxes
            {
                Vector2I Side = new Vector2I(PlayerSide, 1);

                Pushbox.UpdateCollider(FixedPosition + (GetCurrentHitbox().PushboxCenter * Side), GetCurrentHitbox().PushboxSize);
                
                for(int i = 0; i < Hitboxes.Length; i++)
                    if (i < GetCurrentHitbox().Hitboxes.Length)
                        Hitboxes[i].UpdateCollider(FixedPosition + (GetCurrentHitbox().Hitboxes[i].Center * Side), GetCurrentHitbox().Hitboxes[i].Size);
                    else
                        Hitboxes[i].UpdateCollider(FixedPosition, Vector2I.Zero);
            }
        }

        public HitboxSettings GetCurrentHitbox() => HitboxPresets[CurrentHitbox];

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(FixedPosition.X);
            bw.Write(FixedPosition.Y);
            bw.Write(FixedVelocity.X);
            bw.Write(FixedVelocity.Y);
            bw.Write(IsLeftSide);
            bw.Write(PlayerSide);
            bw.Write(IsMovable);
            bw.Write(HitConfirmed);
            bw.Write(ProximityBlocked);
            bw.Write(CurrentHitbox);
            bw.Write(FrameProperties);
        }

        public void Deserialize(BinaryReader br)
        {
            FixedPosition.X = br.ReadInt32();
            FixedPosition.Y = br.ReadInt32();
            FixedVelocity.X = br.ReadInt32();
            FixedVelocity.Y = br.ReadInt32();
            IsLeftSide = br.ReadBoolean();
            PlayerSide = br.ReadInt32();
            IsMovable = br.ReadBoolean();
            HitConfirmed = br.ReadBoolean();
            ProximityBlocked = br.ReadBoolean();
            CurrentHitbox = br.ReadInt32();
            FrameProperties = br.ReadByte();
        }
    }
}
