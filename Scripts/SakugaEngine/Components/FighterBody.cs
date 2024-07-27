using Godot;
using System.IO;
using SakugaEngine.Resources;
using SakugaEngine.Collision;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FighterBody : SakugaNode, IDamage
    {
        [ExportCategory("Components")]
        [Export] public PhysicsBody Body;
        [Export] public InputManager Inputs;
        [Export] public FighterVariables Variables;
        [Export] public FrameAnimator Animator;
        [Export] public StanceManager Stance;
        [Export] public CombatTracker Tracker;
    
        [ExportCategory("Timers")]
        [Export] public FrameTimer HitStun;
        [Export] public FrameTimer HitStop;
        [Export] public FrameTimer  MoveBuffer;
        [Export] public FrameTimer  PushForce;
        [Export] public FrameTimer  HorizontalBounce;
        [Export] public FrameTimer  VerticalBounce;

        [ExportCategory("Visuals")]
        [Export] public FighterProfile Profile;
        [Export] private Node3D[] Graphics;

        //[ExportCategory("Variables")]
        public bool SuperStop;
        public int LayerSorting = -1;

        private bool PushAllowInertia;
        private int PushGravity;
        private int HBounceIntensity;
        private int VBounceIntensity;
        private int HitstunType = -1;
        
        private FighterBody _opponent;

        public FighterBody GetOpponent() => _opponent;
        public void SetOpponent(FighterBody opponent) { _opponent = opponent; }

        public override void _Process(double delta)
        {
            Position = Global.ToScaledVector3(Body.FixedPosition);
            foreach (Node3D g in Graphics)
                g.Scale = new Vector3(Body.IsLeftSide ? 1 : -1, 1, 1);
        }

        public void ParseInputs(ushort rawInputs)
        {
            Inputs.InsertToHistory(rawInputs);
        }

        public void Initialize(int index)
        {
            Body.Initialize(this);
            Stance.Initialize(this);
            Variables.Initialize();
            Body.FixedPosition.X = Global.StartingPosition * (-1 + (index * 2));
            CallState(Stance.GetCurrentStance().NeutralState);
            Animator.Frame = -1;
        }

        public void Reset(int index)
        {
            Body.FixedVelocity = Vector2I.Zero;
            Body.FixedPosition.X = Global.StartingPosition * (-1 + (index * 2));
            Body.FixedPosition.Y = 0;
            if (!Stance.GetCurrentStance().IsPersistent)
                Stance.CurrentStance = 0;
            CallState(Stance.GetCurrentStance().NeutralState);
            Variables.Initialize();
            Animator.Frame = -1;
            HitStun.Stop();
            HitStop.Stop();
            MoveBuffer.Stop();
            PushForce.Stop();
            HorizontalBounce.Stop();
            VerticalBounce.Stop();
        }

        public void ChangeSide(bool leftSide)
        {
            if (Body.IsLeftSide == leftSide) return;
            if (!Body.IsOnGround) return;
            if (!Stance.CanAutoTurn()) return;

            Body.IsLeftSide = leftSide;

            if (Animator.GetCurrentState().TurnState >= 0)
                CallState(Animator.GetCurrentState().TurnState);

            if (Stance.currentMove >= 0 &&(int)Stance.GetCurrentMove().SideChange == 2)
                Stance.ResetStance();
            
            
        }

        public void Tick()
        {
            Inputs.InputSide = Body.IsLeftSide ? 1 : -1;
            
            HitStop.Run();
            if (!HitStop.IsRunning())
            {
                HitStun.Run();
                PushForce.Run();
                HorizontalBounce.Run();
                VerticalBounce.Run();
                Animator.RunState();
                StateTransitions();
                Animator.LoopState();
            }
            MoveBuffer.Run();
            Stance.CheckMoves();
            UpdateHitboxes();
            if (PushForce.IsRunning())
                CharacterPushing();
            else
                UpdateFighterPhysics();
        }

        private void UpdateFighterPhysics()
        {
            if (Animator.GetCurrentState().statePhysics.Length == 0) return;

            for(int i = 0; i < Animator.GetCurrentState().statePhysics.Length; ++i)
            {
                int nextFrame = i + 1 < Animator.GetCurrentState().statePhysics.Length ? 
                                        Animator.GetCurrentState().statePhysics[i + 1].frame : 
                                        Animator.GetCurrentState().Duration;
                if (Animator.Frame >= Animator.GetCurrentState().statePhysics[i].frame && Animator.Frame < nextFrame)
                {
                    if (Animator.GetCurrentState().statePhysics[i].UseLateralSpeed)
                        Body.SetLateralVelocity(Animator.GetCurrentState().statePhysics[i].LateralSpeed * Inputs.InputSide);
                    if (Animator.GetCurrentState().statePhysics[i].UseVerticalSpeed)
                        Body.SetVerticalVelocity(Animator.GetCurrentState().statePhysics[i].VerticalSpeed);
                    if (Animator.GetCurrentState().statePhysics[i].UseGravity)
                        Body.AddGravity(Animator.GetCurrentState().statePhysics[i].Gravity);
                }
            }
        }

        public void UpdateHitboxes()
        {
            for (int i = 0; i < Animator.GetCurrentState().hitboxStates.Length; ++i)
            {
                if (Animator.Frame == Animator.GetCurrentState().hitboxStates[i].Frame)
                {
                    Body.CurrentHitbox = Animator.GetCurrentState().hitboxStates[i].HitboxIndex;
                    Body.HitConfirmed = false;
                }
            }
        }

        public void CallState(int index, bool reset = false)
        {
            Animator.PlayState(index, reset);
        }

        public void StateTransitions()
        {
            if (Animator.GetCurrentState().stateTransitions.Length <= 0) return;

            bool ValidTransition = false;

            for (int i = 0; i < Animator.GetCurrentState().stateTransitions.Length; i++)
            {
                if (Animator.GetCurrentState().stateTransitions[i].StateIndex < 0) continue;
                
                switch ((int)Animator.GetCurrentState().stateTransitions[i].Condition)
                {
                    case 0: //State End
                        ValidTransition = Animator.Frame >= Animator.GetCurrentState().Duration;
                        break;
                    case 1: //At Frame
                        ValidTransition = Animator.Frame >= Animator.GetCurrentState().stateTransitions[i].AtFrame;
                        break;
                    case 2: //On Ground
                        ValidTransition = Body.IsOnGround;
                        break;
                    case 3: //On Walls
                        ValidTransition = Body.IsOnWall;
                        break;
                    case 4: //On Fall
                        ValidTransition = Body.FixedVelocity.Y < 0;
                        break;
                    case 5: //On K.O. (For Fighters)
                        ValidTransition = Variables.CurrentHealth == 0;
                        break;
                    case 6: //On Lifetime End (For Projectiles)
                        //TODO
                        break;
                    case 7: //On Input Command
                        ValidTransition = Inputs.CheckMotionInputs(Animator.GetCurrentState().stateTransitions[i].Inputs);
                        break;
                }

                if (ValidTransition) CallState(Animator.GetCurrentState().stateTransitions[i].StateIndex);
            }
        }

#region Push Force
        public void PushCharacter(int pushDuration, int VelocityX, int VelocityY, int gravity, bool xInertia)
        {
            
            Body.FixedVelocity.X = VelocityX;
            Body.FixedVelocity.Y = VelocityY;
            PushGravity = gravity;
            PushAllowInertia = xInertia;
            PushForce.Start((uint)pushDuration);
        }

        public void HitPushback(int duration, int velX, bool allowPushback, int pushbackTimerAdditional)
        {
            int pushbackSide = Body.FixedPosition.X > 0 ? 1 : -1;
            if (Body.IsOnGround)
                if (GetOpponent().Body.IsOnWall && allowPushback)
                    PushCharacter(duration + pushbackTimerAdditional, velX * pushbackSide, 0, 0, false);
        }

        public void CharacterPushing()
        {
            if (PushGravity != 0 && !HitStop.IsRunning() && !Body.IsOnGround)
            {
                Body.AddGravity(PushGravity / Global.TicksPerSecond);
            }

            if (!PushForce.IsRunning())
            {
                if(PushGravity != 0) PushGravity = 0;
                if(Body.IsOnGround || !PushAllowInertia) Body.FixedVelocity.X = 0;
            }
        }

        public void BounceLogic()
        {
            if (Body.IsOnWall && HorizontalBounce.IsRunning())
            {
                Body.FixedVelocity.X *= HBounceIntensity * -1;
                Body.FixedVelocity.X /= 100;
                HorizontalBounce.Stop();
                //Debug.LogWarning("Bounced on wall!");

            }
            if (Body.IsOnGround && Body.IsFalling && VerticalBounce.IsRunning())
            {
                Body.FixedVelocity.Y *= VBounceIntensity * -1;
                Body.FixedVelocity.Y /= 100;
                VerticalBounce.Stop();
                //Debug.LogWarning("Bounced on ground!");
            }
        }

        public void ThrowPivoting()
        {
            if (Animator.GetCurrentState().statePhysics.Length == 0) return;
            for(int i = 0; i < GetOpponent().Animator.GetCurrentState().throwPivot.Length; i++)
            {
                int nextFrame = i + 1 < Animator.GetCurrentState().throwPivot.Length ?
                                    Animator.GetCurrentState().throwPivot[i + 1].Frame :
                                    Animator.GetCurrentState().Duration;
                if (Animator.Frame >= Animator.GetCurrentState().throwPivot[i].Frame && Animator.Frame < nextFrame)
                {
                    int side = GetOpponent().Body.IsLeftSide ? 1 : -1;
                    Body.FixedPosition.X = GetOpponent().Body.FixedPosition.X + GetOpponent().Animator.GetCurrentState().throwPivot[i].PivotPosition.X * side;
                    Body.FixedPosition.Y = GetOpponent().Body.FixedPosition.Y + GetOpponent().Animator.GetCurrentState().throwPivot[i].PivotPosition.Y;
                }
            }

            //Push opponent away from the wall if both characters are too near from each other
            int pushbackSide = Body.FixedPosition.X > 0 ? 1 : -1;
            if (Mathf.Abs(Body.FixedPosition.X - GetOpponent().Body.FixedPosition.X) <= 5)
                GetOpponent().Body.FixedPosition.X -= 5 * pushbackSide;
        }
#endregion
        /// <summary>
        /// Calls block states
        /// The block type moves the state index to the designed block type
        /// 0 = Enter blocking state
        /// 1 = Blocking state
        /// 2 = Exiting block state
        /// 3 = Guard Break
        /// </summary>
        public void CallBlockState(int blockType)
        {
            if (Body.IsOnGround)
            {
                if (IsCrouching())
                {
                    if (Stance.GetCurrentStance().CrouchBlockInitialState < 0) return;
                    Animator.PlayState(Stance.GetCurrentStance().CrouchBlockInitialState + blockType, true);
                }
                else
                {
                    if (Stance.GetCurrentStance().GroundBlockInitialState < 0) return;
                    Animator.PlayState(Stance.GetCurrentStance().GroundBlockInitialState + blockType, true);
                }
            }
            else
            {
                if (Stance.GetCurrentStance().AirBlockInitialState < 0) return;
                Animator.PlayState(Stance.GetCurrentStance().AirBlockInitialState + blockType, true);
            }
        }

        public void HitDamage(HitboxElement box)
        {
            LayerSorting = -1;
            var finalHitstun = Body.IsOnGround ? box.GroundHitStun : box.AirHitStun;
            var finalKnockbackTime = Body.IsOnGround ? box.GroundHitKnockbackTime : box.AirHitKnockbackTime;
            var finalDamageKnockback = Body.IsOnGround ? box.GroundDamageKnockback : box.AirDamageKnockback;
            var finalKnockbackGravity = Body.IsOnGround ? box.GroundDamageKnockbackGravity : box.AirDamageKnockbackGravity;
            var finalHitReaction = Body.IsOnGround ? (IsCrouching() ? box.CrouchHitReaction : box.GroundHitReaction)  : box.AirHitReaction;
            var finalDamage = box.BaseDamage * Variables.CurrentDamageScaling / 100;
            bool CanTech = Animator.StateType() == 4 && !HitStun.IsRunning();
            Body.IsLeftSide = !GetOpponent().Body.IsLeftSide;
            //if (!Body.IsOnGround) airMovementLimit = int.MaxValue;
            Body.IsMovable = false;
            HitstunType = Body.IsOnGround ? (int)box.GroundHitstunType : (int)box.AirHitstunType;
            HitStop.Start((uint)box.HitStopDuration);
            HitStun.Start((uint)finalHitstun);
            HorizontalBounce.Start((uint)box.BounceXTime);
            VerticalBounce.Start((uint)box.BounceYTime);
            HBounceIntensity = box.BounceXIntensity;
            VBounceIntensity = box.BounceYIntensity;
            CallState(Stance.GetCurrentStance().HitReactions[finalHitReaction], true);
            PushCharacter(
                finalKnockbackTime + box.HitStopDuration, 
                finalDamageKnockback.X * (GetOpponent().Body.IsLeftSide ? -1 : 1),
                finalDamageKnockback.Y, 
                finalKnockbackGravity, box.AllowInertia);
            Variables.TakeDamage(
                (uint)finalDamage,
                (uint)box.OpponentMeterGain,
                (uint)box.DamageScalingSubtract,
                box.KillingBlow);
            Tracker.UpdateTrackers((uint)finalDamage, Animator.Frame, (int)box.HitType, CanTech);
        }

        public void BlockHit(HitboxElement box) 
        {
            LayerSorting = -1;
            var finalHitstun = Body.IsOnGround ? box.GroundBlockStun : box.AirBlockStun;
            var finalKnockbackTime = Body.IsOnGround ? box.GroundBlockKnockbackTime : box.AirBlockKnockbackTime;
            var finalDamageKnockback = Body.IsOnGround ? box.GroundBlockKnockback : box.AirBlockKnockback;
            var finalKnockbackGravity = Body.IsOnGround ? box.GroundBlockKnockbackGravity : box.AirBlockKnockbackGravity;
            //var finalHitReaction = Body.IsOnGround ? (IsCrouching() ? box.CrouchHitReaction : box.GroundHitReaction)  : box.AirHitReaction;
            //bool CanTech = StateType() == 3 && meters.StunLevel <= 0;
            Body.IsLeftSide = !GetOpponent().Body.IsLeftSide;
            //if (!Body.IsOnGround) airMovementLimit = int.MaxValue;
            Body.IsMovable = false;
            CallBlockState(1);
            HitStun.Start((uint)finalHitstun);
            PushCharacter(
                finalKnockbackTime + box.HitStopDuration,
                finalDamageKnockback.X * (GetOpponent().Body.IsLeftSide ? -1 : 1),
                finalDamageKnockback.X,
                finalKnockbackGravity, box.AllowInertia);
            Variables.TakeDamage(
                (uint)box.ChipDamage,
                (uint)box.OpponentMeterGain,
                0,
                box.ChipDeath);
        }
        public void ArmorHit(HitboxElement box, Vector2I contact)
        {
            //Armor hit
            HitConfirm((uint)box.SelfMeterGain, (uint)box.ClashHitStopDuration, -1, -1, contact);
            GetOpponent().HitConfirm((uint)box.OpponentMeterGain, (uint)box.ClashHitStopDuration, -1, box.ArmorHitEffectIndex, contact);
            Variables.ArmorDamage((uint)box.ArmorDamage, (uint)box.BaseDamage / 2);
        }
        public void ThrowHit(HitboxElement box) {}

        public void HitConfirm(uint superGaugeGain, uint hitStopDuration, int hitConfirmAnimation, int hitEffect, Vector2I VFXSpawn)
        {
            LayerSorting = 1;
            Body.HitConfirmed = true;
            Body.IsMovable = false;
            Stance.canMoveCancel = true;
            Variables.AddSuperGauge(superGaugeGain);
            HitStop.Start(hitStopDuration);
            /*if (hitEffect >= 0)
            {
                VFXPool[hitEffect].Spawn(VFXSpawn, PlayerSide);
                sounds.QueueSound(sounds.Last, hitEffect);
            }*/

            if (hitConfirmAnimation >= 0)
                CallState(hitConfirmAnimation, false);
        }
#region Damage functions

#endregion

#region Return functions
        public bool IsCrouching() => Body.IsOnGround && Inputs.IsBeingPressed(Inputs.CurrentHistory, Global.INPUT_DOWN);
        public bool IsBlocking() => (int)Animator.GetCurrentState().Type != 2 && (int)Animator.GetCurrentState().Type != 4 && Inputs.IsBeingPressed(Inputs.CurrentHistory, Body.IsLeftSide ? Global.INPUT_LEFT : Global.INPUT_RIGHT);
        public bool IsKO() => Variables.CurrentHealth <= 0;
         public bool IsGroundHit() => Body.IsOnGround && !Animator.GetCurrentState().OffTheGround;
        public bool IsStunLocked() => IsGroundHit() && HitStun.TimeLeft >= Animator.GetCurrentState().Duration - Animator.GetCurrentState().HitStunFrameLimit + 1 && Animator.Frame >= Animator.GetCurrentState().HitStunFrameLimit;

#endregion

#region Interface functions
        public void BaseDamage(HitboxElement box, Vector2I contact)
        {
            if (Variables.SuperArmor > 0)
            {
                ArmorHit(box, contact);
                GD.Print("Fighter: Armor Hit");
            }
            else
            {
                if (IsBlocking())
                {
                    GetOpponent().BlockHit(box);
                    GD.Print("Fighter: Blocked!");
                }
                else
                {
                    GetOpponent().HitDamage(box);
                    GD.Print("Fighter: Hit!");
                }

                HitConfirm((uint)box.SelfMeterGain, (uint)box.HitStopDuration, box.HitConfirmState, box.HitEffectIndex, contact);
                HitPushback(box.SelfPushbackDuration + box.HitStopDuration, box.SelfPushbackForce, box.AllowSelfPushback, 0);
            }
        }
        public void ThrowDamage(HitboxElement box){}
        public void ProjectileDamage(HitboxElement box, Vector2I contact, int priority){}
        public void HitboxClash(HitboxElement box, Vector2I contact, int priority){}
        public void ProjectileDeflect(HitboxElement box){}
        public void CounterHit(HitboxElement box, Vector2I contact){}
        public void ProximityBlock()
        {
            CallBlockState(0);
        }
        public void OnHitboxExit(){}
#endregion

#region Game State
        public override void Serialize(BinaryWriter bw)
        {
            //Components
            Body.Serialize(bw);
            Inputs.Serialize(bw);
            Variables.Serialize(bw);
            Animator.Serialize(bw);
            Stance.Serialize(bw);
            Tracker.Serialize(bw);
            //Timers
            HitStun.Serialize(bw);
            HitStop.Serialize(bw);
            MoveBuffer.Serialize(bw);
            PushForce.Serialize(bw);
            HorizontalBounce.Serialize(bw);
            VerticalBounce.Serialize(bw);
            //Variables
            bw.Write(SuperStop);
            bw.Write(PushAllowInertia);

            bw.Write(PushGravity);
            bw.Write(HBounceIntensity);
            bw.Write(VBounceIntensity);
            bw.Write(HitstunType);
        }

        public override void Deserialize(BinaryReader br)
        {
            //Components
            Body.Deserialize(br);
            Inputs.Deserialize(br);
            Variables.Deserialize(br);
            Animator.Deserialize(br);
            Stance.Deserialize(br);
            Tracker.Deserialize(br);
            //Timers
            HitStun.Deserialize(br);
            HitStop.Deserialize(br);
            MoveBuffer.Deserialize(br);
            PushForce.Deserialize(br);
            HorizontalBounce.Deserialize(br);
            VerticalBounce.Deserialize(br);
            //Variables
            SuperStop = br.ReadBoolean();
            PushAllowInertia = br.ReadBoolean();

            PushGravity = br.ReadInt32();
            HBounceIntensity = br.ReadInt32();
            VBounceIntensity = br.ReadInt32();
            HitstunType = br.ReadInt32();

            Body.UpdateColliders();
        }
#endregion
    }
}
