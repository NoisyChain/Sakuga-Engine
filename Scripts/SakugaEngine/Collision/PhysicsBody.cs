using Godot;
using SakugaEngine.Resources;
using System.Collections.Generic;
using SakugaEngine.Global;

namespace SakugaEngine.Collision
{
    [GlobalClass]
    public partial class PhysicsBody : Node
    {
        private SakugaActor _owner;
        [Export] public bool IsStatic;
        [Export] public bool StayOnBounds;
        [Export] public bool IgnoreWalls;
        [Export] public int HitboxesLimit = 16;

        public IDamage Parent;
        public uint ID;
        public Collider Pushbox;
        public Collider[] Hitboxes;

        public Vector2I FixedPosition;
        public Vector2I FixedVelocity;
        public bool IsLeftSide;
        public int PlayerSide;
        public bool IsMovable = true;
        public List<uint> HitBodies = new List<uint>();
        public bool ProximityBlocked;
        public HitboxState CurrentHitbox = null;
        public int CurrentGravity = GlobalVariables.DefaultGravity;
        
        public bool IsOnGround => FixedPosition.Y <= 0;
        public bool IsOnLeftWall => FixedPosition.X <= -GlobalVariables.WallLimit;
        public bool IsOnRightWall => FixedPosition.X >= GlobalVariables.WallLimit;
        public bool IsFalling => !IsOnGround && FixedVelocity.Y <= 0;
        public bool JustLanded => IsOnGround && FixedVelocity.Y < 0;
        public bool IsOnWall => IsOnLeftWall || IsOnRightWall;

        public void Initialize(SakugaActor newOwner, IDamage newParent)
        {
            _owner = newOwner;
            Parent = newParent;
            FixedPosition = Vector2I.Zero;
            FixedVelocity = Vector2I.Zero;
            Hitboxes = new Collider[HitboxesLimit];
        }
        public void SetID(uint newID)
        {
            ID = newID;
        }

        public void UpdateSide(bool leftSide)
        {
            IsLeftSide = leftSide;
        }

        public void FlipSide()
        {
            PlayerSide = IsLeftSide ? 1 : -1;
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
                    FixedVelocity.X += GlobalVariables.DefaultFriction / GlobalVariables.TicksPerSecond;
                else if (FixedVelocity.X < absVelocity)
                    FixedVelocity.X += GlobalVariables.DefaultAcceleration / GlobalVariables.TicksPerSecond;
                else
                    FixedVelocity.X = absVelocity;
                    
            }
            else if (newVelocity != 0 && Mathf.Sign(newVelocity) < 0)
            {
                if (FixedVelocity.X > 0)
                    FixedVelocity.X -= GlobalVariables.DefaultFriction / GlobalVariables.TicksPerSecond;
                else if (FixedVelocity.X > -absVelocity)
                    FixedVelocity.X -= GlobalVariables.DefaultAcceleration / GlobalVariables.TicksPerSecond;
                else
                    FixedVelocity.X = -absVelocity;
            }
            else
                FixedVelocity.X -= Mathf.Min(Mathf.Abs(FixedVelocity.X), GlobalVariables.DefaultDeceleration / GlobalVariables.TicksPerSecond) * Mathf.Sign(FixedVelocity.X);
        }
        public void AddVerticalAcceleration(int newVelocity)
        {
            int absVelocity = Mathf.Abs(newVelocity);
            if (newVelocity != 0 && Mathf.Sign(newVelocity) > 0)
            {
                if (FixedVelocity.Y < 0)
                    FixedVelocity.Y += GlobalVariables.DefaultFriction / GlobalVariables.TicksPerSecond;
                else if (FixedVelocity.Y < absVelocity)
                    FixedVelocity.Y += GlobalVariables.DefaultAcceleration / GlobalVariables.TicksPerSecond;
                else
                    FixedVelocity.Y = absVelocity;
                    
            }
            else if (newVelocity != 0 && Mathf.Sign(newVelocity) < 0)
            {
                if (FixedVelocity.Y > 0)
                    FixedVelocity.Y -= GlobalVariables.DefaultFriction / GlobalVariables.TicksPerSecond;
                else if (FixedVelocity.Y > -absVelocity)
                    FixedVelocity.Y -= GlobalVariables.DefaultAcceleration / GlobalVariables.TicksPerSecond;
                else
                    FixedVelocity.Y = -absVelocity;
            }
            else
                FixedVelocity.Y -= Mathf.Min(Mathf.Abs(FixedVelocity.Y), GlobalVariables.DefaultDeceleration / GlobalVariables.TicksPerSecond) * Mathf.Sign(FixedVelocity.Y);
        }
        public void AddGravity()
        {
            FixedVelocity.Y -= CurrentGravity / GlobalVariables.TicksPerSecond;
        }
        public void MoveTo(Vector2I destination)
        {
            if (IsStatic) return;
            
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

            FixedPosition += FixedVelocity / GlobalVariables.Delta;
            UpdateColliders();
        }

        public void AddHitBody(PhysicsBody body)
        {
            HitBodies.Add(body.ID);
        }

        public void ResetHits()
        {
            HitBodies.Clear();
        }

        public bool CanHitBody(PhysicsBody body)
        {
            return !HitBodies.Contains(body.ID);
        }

        public void BounceX(int intensity)
        {
            FixedVelocity.X *= -intensity;
            FixedVelocity.X /= 100;
            GD.Print($"{FixedVelocity.X}, {intensity}");
        }

        public void BounceY(int intensity)
        {
            FixedVelocity.Y *= -intensity;
            FixedVelocity.Y /= 100;
        }

        public void UpdateColliders()
        {
            CurrentHitbox = GetCurrentHitboxSettings();
            //Lock collider on bounds
            if (StayOnBounds)
            {
                if (!IgnoreWalls)
                    FixedPosition.X = Mathf.Clamp(FixedPosition.X, -GlobalVariables.WallLimit, GlobalVariables.WallLimit);
                FixedPosition.Y = Mathf.Clamp(FixedPosition.Y, 0, GlobalVariables.CeilingLimit);
            }

            if (CurrentHitbox == null || CurrentHitbox.HitboxData == null) //If there's no hitbox active, remove all boxes
            {
                Pushbox.UpdateCollider(FixedPosition, Vector2I.Zero);

                for(int i = 0; i < Hitboxes.Length; i++)
                    Hitboxes[i].UpdateCollider(FixedPosition, Vector2I.Zero);
            }
            else //Update hitboxes
            {
                Vector2I Side = new Vector2I(PlayerSide, 1);

                Pushbox.UpdateCollider(FixedPosition + (CurrentHitbox.HitboxData.Pushbox.Center * Side), CurrentHitbox.HitboxData.Pushbox.Size);
                
                for(int i = 0; i < Hitboxes.Length; i++)
                    if (i < CurrentHitbox.HitboxData.Hitboxes.Length)
                        Hitboxes[i].UpdateCollider(FixedPosition + (CurrentHitbox.HitboxData.Hitboxes[i].Center * Side), CurrentHitbox.HitboxData.Hitboxes[i].Size);
                    else
                        Hitboxes[i].UpdateCollider(FixedPosition, Vector2I.Zero);
            }
        }

        public HitboxState GetCurrentHitboxSettings()
        {
            var State = _owner.StateManager.GetCurrentState();
            if (State.AnimationData == null) return null;
            if (State.AnimationData.Hitboxes == null || State.AnimationData.Hitboxes.Length <= 0) return null;
            if (State.AnimationData.Hitboxes.Length == 1)
                return State.AnimationData.Hitboxes[0];

            int anim = 0;

            for (int i = 0; i < State.AnimationData.Hitboxes.Length; i++)
            {
                int nextFrame = (i == State.AnimationData.Hitboxes.Length - 1) ?
                                int.MaxValue :
                                State.AnimationData.Hitboxes[i + 1].AtFrame - 1;
                
                if (_owner.StateManager.CurrentStateFrame >= State.AnimationData.Hitboxes[i].AtFrame && _owner.StateManager.CurrentStateFrame <= nextFrame)
                    anim = i;
            }

            return State.AnimationData.Hitboxes[anim];
        }
    }
}
