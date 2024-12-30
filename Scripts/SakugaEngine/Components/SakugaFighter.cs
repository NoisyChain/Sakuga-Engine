using Godot;
using System.IO;
using SakugaEngine.Resources;
using SakugaEngine.Collision;

namespace SakugaEngine
{
    [GlobalClass]
    [Icon("res://Sprites/Icons/Icon_Fighter.png")]
    public partial class SakugaFighter : SakugaActor, IDamage
    {
        [ExportCategory("Timers")]
        [Export] public FrameTimer HitStun;
        [Export] public FrameTimer HitStop;
        [Export] public FrameTimer  MoveBuffer;
        [Export] public FrameTimer  PushForce;
        [Export] public FrameTimer  HorizontalBounce;
        [Export] public FrameTimer  VerticalBounce;

        [ExportCategory("Lists")]
        [Export] public SpawnsList SpawnablesList;

        [ExportCategory("Extras")]
        [Export] public FighterProfile Profile;

        public bool SuperStop;
        public int LayerSorting = -1;

        private bool PushAllowInertia;
        private bool IsBeingPushed = false;
        private bool BlockStun;
        private bool Uncrouched;
        private int PushGravity;
        private int HBounceIntensity;
        private int HBounceState;
        private int VBounceIntensity;
        private int VBounceState;
        private byte HitstunType = 0;
        private byte GravityDecayFactor = 0;
        private byte HitstunDecayFactor = 0;
        private SakugaSpawnable[][] Spawnables;
        private SakugaVFX[][] VFX;
        
        private SakugaFighter _opponent;

        public SakugaFighter GetOpponent() => _opponent;
        public void SetOpponent(SakugaFighter opponent) { if (opponent != _opponent) _opponent = opponent; }
        public FighterVariables FighterVars => Variables as FighterVariables;

        public override void _Process(double delta)
        {
            base._Process(delta);
        }

        public void ParseInputs(ushort rawInputs)
        {
            Inputs.InsertToHistory(rawInputs);
        }

        public void SpawnablesSetup(Node Parent, PhysicsWorld world)
        {
            Spawnables = new SakugaSpawnable[SpawnablesList.SpawnObjects.Length][];
            for (int i = 0; i < Spawnables.Length; ++i)
            {
                Spawnables[i] = new SakugaSpawnable[SpawnablesList.SpawnObjects[i].Ammount];
                for (int j = 0; j < Spawnables[i].Length; ++j)
                {
                    Node temp = SpawnablesList.SpawnObjects[i].SpawnScene.Instantiate();
                    Parent.AddChild(temp);
                    Spawnables[i][j] = temp as SakugaSpawnable;
                    world.AddBody(Spawnables[i][j].Body);
                    Spawnables[i][j].Initialize(this);
                }
            }
        }

        public void VFXSetup(Node Parent)
        {
            VFX = new SakugaVFX[VFXList.SpawnObjects.Length][];
            for (int i = 0; i < VFX.Length; ++i)
            {
                VFX[i] = new SakugaVFX[VFXList.SpawnObjects[i].Ammount];
                for (int j = 0; j < VFX[i].Length; ++j)
                {
                    Node temp = VFXList.SpawnObjects[i].SpawnScene.Instantiate();
                    Parent.AddChild(temp);
                    VFX[i][j] = temp as SakugaVFX;
                    VFX[i][j].Initialize();
                }
            }
        }

        public void Initialize(int index)
        {
            Body.Initialize(this);
            Stance.Initialize(this);
            Tracker.Initialize(this);
            Variables.Initialize();

            BlockStun = false;
            Uncrouched = false;
            
            Body.FixedPosition.X = Global.StartingPosition * (-1 + (index * 2));
            Animator.PlayState(Stance.GetCurrentStance().NeutralState);
            Animator.Frame = -1;
        }

        public void Reset(int index)
        {
            Body.FixedVelocity = Vector2I.Zero;
            Body.FixedPosition.X = Global.StartingPosition * (-1 + (index * 2));
            Body.FixedPosition.Y = 0;
            if (!Stance.GetCurrentStance().IsRoundPersistent)
                Stance.CurrentStance = 0;
            Animator.PlayState(Stance.GetCurrentStance().NeutralState);
            Variables.Initialize();
            Animator.Frame = -1;
            HitStun.Stop();
            HitStop.Stop();
            MoveBuffer.Stop();
            PushForce.Stop();
            HorizontalBounce.Stop();
            VerticalBounce.Stop();

            BlockStun = false;
            Uncrouched = false;
        }

        public void UpdateSide(bool leftSide)
        {
            if (Body.IsLeftSide == leftSide) return;

            Body.IsLeftSide = leftSide;
        }

        public void ChangePlayerSide()
        {
            if (Body.IsLeftSide && Body.PlayerSide > 0) return;
            if (!Body.IsLeftSide && Body.PlayerSide < 0) return;
            
            if (!Body.IsOnGround) return; //Never auto turn on air
            if (Animator.StateType() != 1) return; // Also only auto turn in movement states
            if (!Stance.CanAutoTurn()) return; //If a move is being executed, leave the decision to it

            Body.PlayerSide = Body.IsLeftSide ? 1 : -1;

            if (Animator.GetCurrentState().TurnState >= 0)
                Animator.PlayState(Animator.GetCurrentState().TurnState);

            if (Stance.CurrentMove >= 0 &&(int)Stance.GetCurrentMove().SideChange == 2)
                Stance.ResetStance();
        }

        public void ForcePlayerSide()
        {
            Body.PlayerSide = Body.IsLeftSide ? 1 : -1;
        }

        public override void PreTick()
        {
            HitStop.Run();
            EventExecuted = false;
            Body.IsMovable = !HitStop.IsRunning();

            if (!HitStop.IsRunning())
            {
                if (SuperStop) SuperStop = false;
                if (HitstunType != (int)Global.HitstunType.HARD_KNOCKDOWN)
                    HitStun.Run();
                PushForce.Run();
                HorizontalBounce.Run();
                VerticalBounce.Run();
                MoveBuffer.Run();
                if (Animator.GetCurrentState().HitStunFrameLimit < 0 || !IsStunLocked())
                    Animator.RunState();
            }

            for (int i = 0; i < Spawnables.Length; ++i)
                for (int j = 0; j < Spawnables[i].Length; ++j)
                    Spawnables[i][j].PreTick();
        }

        public override void Tick()
        {
            ThrowEscape();
            HitstunRecover();
            BounceLogic();

            if (HitstunType > (int)Global.HitstunType.STAGGER)
                ThrowPivoting();
            else
                if (IsBeingPushed)
                    CharacterPushing();
                else
                    UpdateFighterPhysics();
            
            Stance.CheckMoves();

            for (int i = 0; i < Spawnables.Length; ++i)
                for (int j = 0; j < Spawnables[i].Length; ++j)
                    Spawnables[i][j].Tick();
        }

        public override void LateTick()
        {
            ChangePlayerSide();
            Inputs.InputSide = Body.PlayerSide;
            Variables.UpdateExtraVariables();
            FighterVars.CalculateDamageScaling(Body.IsOnWall);
            Tracker.UpdateFrameData();

            UpdateFrameProperties();
            StateTransitions();
            AnimationEvents();
            UpdateHitboxes();
            ResetDamageStatus();

            for (int i = 0; i < Spawnables.Length; ++i)
                for (int j = 0; j < Spawnables[i].Length; ++j)
                    Spawnables[i][j].LateTick();

            for (int i = 0; i < VFX.Length; ++i)
                for (int j = 0; j < VFX[i].Length; ++j)
                    VFX[i][j].Tick();
        }

        private void ResetDamageStatus()
        {
            if (HitstunType < (int)Global.HitstunType.STAGGER && 
                Animator.StateType() != (int)Global.StateType.HIT_REACTION)
            {
                HitStun.Stop(); 
                FighterVars.ResetDamageStatus();
                Tracker.Reset();
                HitstunDecayFactor = 0;
                GravityDecayFactor = 0;
                HitstunType = 0;
                BlockStun = false;
                Uncrouched = false;
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
            IsBeingPushed = true;
        }

        public void HitPushback(int duration, int velX)
        {
            int pushbackSide = Body.FixedPosition.X > 0 ? 1 : -1;
            if (Body.IsOnGround)
                if (GetOpponent().Body.IsOnWall)
                    PushCharacter(duration, velX * pushbackSide, 0, 0, false);
        }

        public void CharacterPushing()
        {
            if (PushGravity != 0 && !Body.IsOnGround && !HitStop.IsRunning())
            {
                Body.AddGravity(PushGravity);
            }

            if (!PushForce.IsRunning())
            {
                if(PushGravity != 0) PushGravity = 0;
                if (!PushAllowInertia || Body.IsOnGround) Body.FixedVelocity.X = 0;
                IsBeingPushed = false;
            }
        }

        public void StopPushing()
        {
            if (!IsBeingPushed) return;

            if(PushGravity != 0) PushGravity = 0;
            if (!PushAllowInertia || Body.IsOnGround) Body.FixedVelocity.X = 0;
            IsBeingPushed = false;
            PushForce.Stop();
        }

        public void BounceLogic()
        {
            bool canBounceX = HorizontalBounce.IsRunning() && HBounceState >= 0;
            bool canBounceY = VerticalBounce.IsRunning() && VBounceState >= 0;

            if (Body.IsOnWall && canBounceX)
            {
                Animator.PlayState(HBounceState, true);
                Body.FixedVelocity.X *= HBounceIntensity * -1;
                Body.FixedVelocity.X /= 100;
                HorizontalBounce.Stop();
                //Debug.LogWarning("Bounced on wall!");

            }
            if (Body.IsOnGround && Body.IsFalling && canBounceY)
            {
                Animator.PlayState(VBounceState, true);
                Body.FixedVelocity.Y *= VBounceIntensity * -1;
                Body.FixedVelocity.Y /= 100;
                VerticalBounce.Stop();
                //Debug.LogWarning("Bounced on ground!");
            }

            //Clear horizontal bounce buffer
            if (!HorizontalBounce.IsRunning() && (HBounceIntensity > 0 || HBounceState >= 0))
            {
                HBounceState = -1;
                HBounceIntensity = 0;
            }

            //Clear vertical bounce buffer
            if (!VerticalBounce.IsRunning() && (VBounceIntensity > 0 || VBounceState >= 0))
            {
                VBounceState = -1;
                VBounceIntensity = 0;
            }
        }

        public void ThrowPivoting()
        {
            FighterState currState = GetOpponent().Animator.GetCurrentState();
            if (currState.throwPivot.Length == 0) return;
            for(int i = 0; i < currState.throwPivot.Length; i++)
            {
                int nextFrame = i + 1 < currState.throwPivot.Length ?
                                    currState.throwPivot[i + 1].Frame :
                                    currState.Duration;

                ThrowPivot currentPivot = currState.throwPivot[i];
                if (Animator.Frame >= currentPivot.Frame && Animator.Frame < nextFrame)
                {
                    int side = GetOpponent().Body.PlayerSide;
                    int hitReaction = currentPivot.ThrowState;
                    if (hitReaction >=  0)
                        Animator.PlayState(Stance.GetCurrentStance().HitReactions[hitReaction], true);
                    if (!currentPivot.Dettach)
                    {
                        Body.FixedVelocity = Vector2I.Zero;
                        Body.FixedPosition.X = GetOpponent().Body.FixedPosition.X + currentPivot.PivotPosition.X * side;
                        Body.FixedPosition.Y = GetOpponent().Body.FixedPosition.Y + currentPivot.PivotPosition.Y;
                    }
                    else
                    {
                        Body.PlayerSide = currentPivot.DettachInvertSide ? -side : side;
                        HitstunType = (byte)currentPivot.DettachHitstunType;
                        HitStun.Start((uint)currentPivot.DettachHitstun);
                        PushCharacter(
                            currentPivot.DettachHitKnockbackTime,
                            currentPivot.DettachHitKnockback.X * side,
                            currentPivot.DettachHitKnockback.Y,
                            currentPivot.DettachHitKnockbackGravity,
                            currentPivot.DettachHitKnockbackInertia
                        );
                    }
                }
            }

            //Push opponent away from the wall if both characters are too near from each other
            int pushbackSide = Body.FixedPosition.X > 0 ? 1 : -1;
            if (Mathf.Abs(Body.FixedPosition.X - GetOpponent().Body.FixedPosition.X) <= 5)
                GetOpponent().Body.FixedPosition.X -= 5 * pushbackSide;
        }

        public void HitstunRecover()
        {
            if (HitStun.IsRunning()) return;
            if (Animator.StateType() != (int)Global.StateType.HIT_REACTION) return;
            if (HitstunType >= (int)Global.HitstunType.HARD_KNOCKDOWN) return;

            int selectedRecovery = -1;
            int recoveryDirection;
            if (Inputs.WasPressed(Inputs.CurrentHistory, Global.INPUT_ANY_BUTTON))
            {
                if (Inputs.IsBeingPressed(Inputs.CurrentHistory, Global.INPUT_LEFT))
                    recoveryDirection = -1;
                else if (Inputs.IsBeingPressed(Inputs.CurrentHistory, Global.INPUT_RIGHT))
                    recoveryDirection = 1;
                else 
                    recoveryDirection = Mathf.Sign(Body.FixedVelocity.X);
                
                if (Body.IsOnGround)
                {
                    if (!Animator.GetCurrentState().OffTheGround)
                    {
                        if (recoveryDirection == 0) recoveryDirection = Body.PlayerSide; 
                        selectedRecovery = recoveryDirection * Body.PlayerSide >= 0 ? 
                                    Stance.GetCurrentStance().GroundForwardRecoveryState :
                                    Stance.GetCurrentStance().GroundBackwardsRecoveryState;
                        PushCharacter(20, Global.RecoveryHorizontalSpeed * recoveryDirection, 0, 0, false);
                    }
                    else if (Animator.GetCurrentState().OffTheGround && HitstunType < (int)Global.HitstunType.KNOCKDOWN)
                    {
                        selectedRecovery = Stance.GetCurrentStance().OffTheGroundRecoveryState;
                        PushCharacter(20, 0, Global.RecoveryJumpVelocity, Global.RecoveryGravity, true);
                    }
                    
                }
                else if (HitstunType < (int)Global.HitstunType.KNOCKDOWN)
                {
                    selectedRecovery = recoveryDirection * Body.PlayerSide >= 0 ? 
                                    Stance.GetCurrentStance().AirForwardRecoveryState :
                                    Stance.GetCurrentStance().AirBackwardsRecoveryState;
                    PushCharacter(20, Mathf.Abs(Body.FixedVelocity.X) * recoveryDirection, Global.RecoveryJumpVelocity, Global.RecoveryGravity, true);
                }
                
                if (selectedRecovery >= 0) 
                {
                    HitstunType = 0;
                    Animator.PlayState(selectedRecovery, true); 
                }
                GD.Print("Recovered!");
            }
        }
#endregion
        /// <summary>
        /// Calls block states. 
        /// The block type moves the state index to the designed block type: 
        /// 0 = Enter blocking state;
        /// 1 = Blocking state;
        /// 2 = Exiting block state;
        /// 3 = Guard Break.
        /// </summary>
        public void CallBlockState(int blockType)
        {
            if (Body.IsOnGround)
            {
                if (IsCrouching())
                {
                    if (Stance.GetCurrentStance().CrouchBlockInitialState >= 0)
                        Animator.PlayState(Stance.GetCurrentStance().CrouchBlockInitialState + blockType, true);
                    
                    GD.Print(Animator.States[Stance.GetCurrentStance().CrouchBlockInitialState + blockType].StateName);
                }
                else
                {
                    if (Stance.GetCurrentStance().GroundBlockInitialState >= 0)
                        Animator.PlayState(Stance.GetCurrentStance().GroundBlockInitialState + blockType, true);
                    
                    GD.Print(Animator.States[Stance.GetCurrentStance().GroundBlockInitialState + blockType].StateName);
                }
            }
            else
            {
                if (Stance.GetCurrentStance().AirBlockInitialState >= 0)
                    Animator.PlayState(Stance.GetCurrentStance().AirBlockInitialState + blockType, true);
                
                GD.Print(Animator.States[Stance.GetCurrentStance().AirBlockInitialState + blockType].StateName);
            }
        }

        public void SpawnSpawnable(int index, Vector2I pos)
        {
            if (Spawnables[index].Length == 1)
                Spawnables[index][0].Spawn(pos);
            else
            {
                for (int i = 0; i < Spawnables[index].Length; ++i)
                {
                    if (Spawnables[index][i].IsActive)
                    {
                        if (i == Spawnables[index].Length - 1)
                        {
                            Spawnables[index][0].Spawn(pos);
                            break;
                        }
                        else continue;
                    }
                    else
                    {
                        Spawnables[index][i].Spawn(pos);
                        break;
                    }
                }
            }
        }

        public SakugaSpawnable GetActiveSpawnable(int index)
        {
            if (Spawnables[index].Length == 1)
                if (Spawnables[index][0].IsActive)
                    return Spawnables[index][0];
            else
            {
                for (int i = 0; i < Spawnables[index].Length; ++i)
                {
                    if (Spawnables[index][i].IsActive)
                    {
                        return Spawnables[index][i];
                    }
                    else continue;
                }
            }

            return null;
        }

        public void SpawnVFX(int index, Vector2I pos)
        {
            if (VFX[index].Length == 1)
                VFX[index][0].Spawn(pos, Body.PlayerSide);
            else
            {
                for (int i = 0; i < VFX[index].Length; ++i)
                {
                    if (VFX[index][i].IsActive)
                    {
                        if (i == VFX[index].Length - 1)
                        {
                            VFX[index][0].Spawn(pos, Body.PlayerSide);
                            break;
                        }
                        else continue;
                    }
                    else
                    {
                        VFX[index][i].Spawn(pos, Body.PlayerSide);
                        break;
                    }
                }
            }
        }

        public string DebugInfo()
        {
            return "Position: "+Body.FixedPosition+
                    "\nVelocity: "+Body.FixedVelocity+
                    "\nStance: "+Stance.CurrentStance+
                    "\nState: "+Animator.CurrentState+
                    "\nAnimation: "+Animator.GetCurrentState().StateName+
                    "\nCurrent Move: "+Stance.CurrentMove+
                    "\nBuffered Move: "+Stance.BufferedMove+
                    "\nFrame: "+Animator.Frame+
                    "\nHitstun Type: "+HitstunType+
                    "\nHitbox: "+Body.CurrentHitbox+
                    "\nHealth: "+Variables.CurrentHealth+"/"+Variables.MaxHealth+
                    "\nSuper Gauge: "+Variables.CurrentSuperGauge+"/"+Variables.MaxSuperGauge+
                    "\nSuper Armor: "+Variables.SuperArmor+
                    "\nHit Stun: "+HitStun.TimeLeft+
                    "\nCharge Buffers: "+Inputs.CurrentInput().hCharge+" | "+Inputs.CurrentInput().vCharge+" | "+Inputs.CurrentInput().bCharge+
                    "\nBlocking: "+IsBlocking();
        }

#region Damage functions
        public void HitDamage(HitboxElement box, bool isTrade)
        {
            LayerSorting = -1;
            if (box.Uncrouch) Uncrouched = true;
            bool isGroundHit = !LifeEnded() && Body.IsOnGround && !Animator.GetCurrentState().OffTheGround;
            var finalHitstun = isGroundHit ? box.GroundHitstun : box.AirHitstun;
            var finalKnockbackTime = isGroundHit ? box.GroundHitKnockbackTime : box.AirHitKnockbackTime;
            var finalDamageKnockback = isGroundHit ? box.GroundHitKnockback : box.AirHitKnockback;
            var finalKnockbackGravity = isGroundHit ? box.GroundHitKnockbackGravity : box.AirHitKnockbackGravity;
            var finalHitReaction = isGroundHit ? (IsCrouching() ? box.CrouchHitReaction : box.GroundHitReaction)  : box.AirHitReaction;
            //var damageFactor = GetOpponent().FighterVars.CurrentAttack - (FighterVars.CurrentDefense - 100);
            //var scaledDamage = box.BaseDamage * FighterVars.CurrentDamageScaling / 100;
            var finalDamage = FighterVars.CalculateCompleteDamage(box.BaseDamage, GetOpponent().FighterVars.CurrentAttack);
            bool CanTech = Animator.StateType() == 4 && !HitStun.IsRunning();
            Body.IsLeftSide = !GetOpponent().Body.IsLeftSide;
            Body.IsMovable = false;
            HitstunType = isGroundHit ? (byte)box.GroundHitstunType : (byte)box.AirHitstunType;
            if (!isTrade)
                HitStop.Start((uint)box.OpponentHitStopDuration);
            else HitStop.Stop();
            uint finalHitstunForReal = (uint)Mathf.Max(finalHitstun - HitstunDecayFactor, Global.MinHitstun);
            HitStun.Start(finalHitstunForReal);
            if (box.BounceXTime > 0)
            {
                HBounceState = Stance.GetCurrentStance().HitReactions[box.BounceXState];
                HBounceIntensity = box.BounceXIntensity;
                HorizontalBounce.Start((uint)box.BounceXTime);
            }
            else
            {
                HBounceState = -1;
                HBounceIntensity = 0;
                HorizontalBounce.Stop();
            }

            if (box.BounceYTime > 0)
            {
                VBounceState = Stance.GetCurrentStance().HitReactions[box.BounceYState];
                VBounceIntensity = box.BounceYIntensity;
                VerticalBounce.Start((uint)box.BounceYTime);
            }
            else
            {
                VBounceState = -1;
                VBounceIntensity = 0;
                VerticalBounce.Stop();
            }
            
            Stance.Clear();
            if (Stance.GetCurrentStance().HitReactions != null && Stance.GetCurrentStance().HitReactions.Length >= 0)
            {
                Animator.PlayState(Stance.GetCurrentStance().HitReactions[finalHitReaction], true);
                Variables.ExtraVariablesOnDamage();
            }
            int decayingKnockback = Global.GravityDecay * GravityDecayFactor;
            PushCharacter(
                finalKnockbackTime, 
                finalDamageKnockback.X * (GetOpponent().Body.IsLeftSide ? -1 : 1),
                finalDamageKnockback.Y - decayingKnockback, 
                finalKnockbackGravity, box.AllowInertia);
            Variables.TakeDamage(
                finalDamage,
                box.OpponentSuperGaugeGain,
                box.KillingBlow);
            if (finalDamage > 0)
            {
                FighterVars.RemoveDamageScaling((ushort)box.DamageScalingSubtract);
            }
            Tracker.UpdateTrackers(
                (uint)finalDamage, GetOpponent().Animator.GetCurrentState().Duration - GetOpponent().Animator.Frame, 
                finalHitstunForReal, (int)box.HitType, CanTech);
            if (Tracker.HitCombo >= Global.HitstunDecayMinCombo)
                HitstunDecayFactor++;
            if (!Body.IsOnGround)
                GravityDecayFactor++;
        }

        public void BlockHit(HitboxElement box) 
        {
            LayerSorting = -1;
            var finalHitstun = Body.IsOnGround ? box.GroundBlockstun : box.AirBlockstun;
            var finalKnockbackTime = Body.IsOnGround ? box.GroundBlockKnockbackTime : box.AirBlockKnockbackTime;
            var finalDamageKnockback = Body.IsOnGround ? box.GroundBlockKnockback : box.AirBlockKnockback;
            var finalKnockbackGravity = Body.IsOnGround ? box.GroundBlockKnockbackGravity : box.AirBlockKnockbackGravity;
            //var damageFactor = GetOpponent().FighterVars.CurrentAttack - (FighterVars.CurrentDefense - 100);
            //var scaledDamage = box.ChipDamage * FighterVars.CurrentDamageScaling / 100;
            var finalDamage = FighterVars.CalculateCompleteDamage(box.ChipDamage, GetOpponent().FighterVars.CurrentAttack);
            //var finalHitReaction = Body.IsOnGround ? (IsCrouching() ? box.CrouchHitReaction : box.GroundHitReaction)  : box.AirHitReaction;
            //bool CanTech = Animator.StateType() == 4 && !HitStun.IsRunning();
            BlockStun = true;
            Body.IsLeftSide = !GetOpponent().Body.IsLeftSide;
            Body.IsMovable = false;
            Stance.Clear();
            HitStun.Start((uint)finalHitstun);
            HitStop.Start((uint)box.BlockStopDuration);
            CallBlockState(1);
            PushCharacter(
                finalKnockbackTime,
                finalDamageKnockback.X * (GetOpponent().Body.IsLeftSide ? -1 : 1),
                finalDamageKnockback.X,
                finalKnockbackGravity, box.AllowInertia);
            Variables.TakeDamage(
                finalDamage,
                box.OpponentSuperGaugeGain,
                box.ChipDeath);
        }
        public void GuardCrush(HitboxElement box)
        {
            //var damageFactor = GetOpponent().FighterVars.CurrentAttack - (FighterVars.CurrentDefense - 100);
            //var scaledDamage = box.GuardCrushDamage * FighterVars.CurrentDamageScaling / 100;
            var finalDamage = FighterVars.CalculateCompleteDamage(box.GuardCrushDamage, GetOpponent().FighterVars.CurrentAttack);
            Body.IsLeftSide = !GetOpponent().Body.IsLeftSide;
            Stance.Clear();
            HitStun.Start(Global.GuardCrushHitstun);
            HitStop.Start((uint)box.BlockStopDuration);
            CallBlockState(3);
            Variables.TakeDamage(
                finalDamage,
                box.OpponentSuperGaugeGain,
                box.KillingBlow);
        }
        public void ArmorHit(HitboxElement box)
        {
            //Armor hit
            var finalDamage = FighterVars.CalculateCompleteDamage(box.BaseDamage, GetOpponent().FighterVars.CurrentAttack);
            HitConfirm(box.OpponentSuperGaugeGain, (uint)box.ClashHitStopDuration, -1, -1, Vector2I.Zero);
            LayerSorting = -1;
            Variables.ArmorDamage((sbyte)box.ArmorDamage, finalDamage / 2);
        }
        public void ThrowHit(HitboxElement box, uint finalHitStop)
        {
            LayerSorting = -1;
            Body.IsLeftSide = !GetOpponent().Body.IsLeftSide;
            Body.IsMovable = false;
            HitstunType = (int)Global.HitstunType.STAGGER + 1; //Always make sure the value is out of the enumerator
            HitStop.Start(finalHitStop);
            Stance.Clear();
            if (Stance.GetCurrentStance().HitReactions != null && Stance.GetCurrentStance().HitReactions.Length >= 0)
            {
                Animator.PlayState(Stance.GetCurrentStance().HitReactions[box.ThrowHitReaction], true);
                Variables.ExtraVariablesOnDamage();
            }
        }
        public void ThrowEscape()
        {
            if (!HitStop.IsRunning()) return;
            if (HitstunType <= (int)Global.HitstunType.STAGGER) return;
            if (Inputs.IsBeingPressed(Inputs.CurrentHistory, Global.INPUT_ANY_BUTTON))
            {
                HitstunType = 0;
                ThrowEscapeAction();
                GetOpponent().ThrowEscapeAction();
            }
        }
        public void ThrowEscapeAction()
        {
            int selfSelectThrowEscape = Body.IsOnGround ? 
                                        Stance.GetCurrentStance().GroundThrowEscapeState : 
                                        Stance.GetCurrentStance().AirThrowEscapeState;
            HitStop.Stop();
            Animator.PlayState(selfSelectThrowEscape);
            PushCharacter(10, -40000 * Body.PlayerSide, 0, 0, true);
        }
        public void HitConfirm(int superGaugeGain, uint hitStopDuration, int hitConfirmAnimation, int hitEffect, Vector2I VFXSpawn)
        {
            LayerSorting = 1;
            Body.HitConfirmed = true;
            Body.IsMovable = false;
            Stance.CanMoveCancel = true;
            Variables.AddSuperGauge(superGaugeGain);
            HitStop.Start(hitStopDuration);
            if (hitEffect >= 0 && VFXSpawn != Vector2I.Zero)
            {
                SpawnVFX(hitEffect, VFXSpawn);
            }

            if (hitConfirmAnimation >= 0)
                Animator.PlayState(hitConfirmAnimation, false);
            
            Variables.ExtraVariablesOnHit();
        }
#endregion

#region Return functions
        public bool IsBlockableState() => Animator.StateType() != 2 && Animator.StateType() != 4;
        public bool IsCrouching() => Body.IsOnGround && !Uncrouched && Inputs.IsBeingPressed(Inputs.CurrentHistory, Global.INPUT_DOWN);
        public bool IsBlocking() => IsBlockableState() && Inputs.IsBeingPressed(Inputs.CurrentHistory, Body.IsLeftSide ? Global.INPUT_LEFT : Global.INPUT_RIGHT);
        public bool IsKO() => Variables.CurrentHealth <= 0;
        public bool IsGroundHit() => Body.IsOnGround && !Animator.GetCurrentState().OffTheGround;
        public bool IsStunLocked() => HitStun.TimeLeft >= Animator.GetCurrentState().Duration - Animator.GetCurrentState().HitStunFrameLimit + 1 && Animator.Frame >= Animator.GetCurrentState().HitStunFrameLimit;
        protected override bool LifeEnded() { return Variables.CurrentHealth <= 0; }
        public override SakugaFighter FighterReference() { return this; }
#endregion

#region Interface functions
        public void BaseDamage(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            SetOpponent(target.FighterReference());

            bool isHitAllowed = !GetOpponent().Body.ContainsFrameProperty((byte)Global.FrameProperties.DAMAGE_IMUNITY);
            if (!isHitAllowed) return;

            bool HitPosition = box.HitType == Global.HitType.UNBLOCKABLE || 
                                box.HitType == Global.HitType.HIGH && GetOpponent().IsCrouching() || 
                                box.HitType == Global.HitType.LOW && !GetOpponent().IsCrouching();
            if (GetOpponent().Variables.SuperArmor > 0)
            {
                GetOpponent().ArmorHit(box);
                HitConfirm(box.SelfSuperGaugeGain, (uint)box.ClashHitStopDuration, -1, box.ArmorHitEffectIndex, contact);
                GD.Print("Fighter: Armor Hit");
            }
            else
            {
                int hitFX;
                if (GetOpponent().IsBlocking() && !HitPosition)
                {
                    if (box.GuardCrush)
                    {
                        hitFX = box.GuardCrushEffectIndex;
                        GetOpponent().GuardCrush(box);
                        GD.Print("Fighter: Guard Crushed!");
                    }
                    else
                    {
                        hitFX = box.BlockEffectIndex;
                        GetOpponent().BlockHit(box);
                        if (box.AllowSelfPushback)
                            HitPushback(box.SelfPushbackDuration, box.SelfPushbackForce);
                        GD.Print("Fighter: Blocked!");
                    }
                }
                else
                {
                    hitFX = box.HitEffectIndex;
                    GetOpponent().HitDamage(box, false);
                    if (box.AllowSelfPushback)
                        HitPushback(box.SelfPushbackDuration, box.SelfPushbackForce);
                    GD.Print("Fighter: Hit!");
                }
                HitConfirm(box.SelfSuperGaugeGain, (uint)box.SelfHitStopDuration, box.HitConfirmState, hitFX, contact);
            }
        }
        public void HitTrade(HitboxElement box, Vector2I contact)
        {
            HitDamage(box, true);
            if (box.HitEffectIndex >= 0 && contact != Vector2I.Zero)
            {
                SpawnVFX(box.HitEffectIndex, contact);
            }
            GD.Print("Fighter: Traded!");
        }
        public void ThrowDamage(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            SetOpponent(target.FighterReference());

            bool isThrowAllowed = !GetOpponent().Body.ContainsFrameProperty((byte)Global.FrameProperties.THROW_IMUNITY) && 
                                    ((GetOpponent().Body.IsOnGround && box.GroundThrow) || 
                                    (!GetOpponent().Body.IsOnGround && box.AirThrow) ||
                                    (box.GroundThrow && box.AirThrow));
            if (!isThrowAllowed) return;

            uint finalHitstop = (uint)box.ThrowHitStopDuration;
            if (GetOpponent().Animator.StateType() == 4) finalHitstop += Global.ThrowHitStunAdditional;
            
            
            GetOpponent().ThrowHit(box, finalHitstop);
            HitConfirm(0, finalHitstop, box.HitConfirmState, -1, Vector2I.Zero);
            GD.Print("Fighter: Throw!");
        }
        public void ThrowTrade()
        {
            ThrowEscapeAction();
        }
        public void HitboxClash(HitboxElement box, Vector2I contact)
        {
            HitConfirm(0, (uint)box.ClashHitStopDuration, -1, box.ClashEffectIndex, contact);
        }
        public void ProjectileClash(HitboxElement box, Vector2I contact){}
        public void ProjectileDeflect(SakugaActor target, HitboxElement box, Vector2I contact){}
        public void CounterHit(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            SetOpponent(target.FighterReference());
            HitConfirm(box.SelfSuperGaugeGain, (uint)box.SelfHitStopDuration, box.HitConfirmState, box.HitEffectIndex, contact);
            GetOpponent().HitConfirm(box.OpponentSuperGaugeGain, (uint)box.OpponentHitStopDuration, -1, -1, Vector2I.Zero);
        }
        public void ProximityBlock()
        {
            Body.ProximityBlocked = true;
            if (IsBlocking() && Animator.StateType() != 3)
            {
                CallBlockState(0);
                Stance.Clear();
            }
        }
        public void OnHitboxExit()
        {
            if (!BlockStun && Animator.StateType() == 3)
            {
                CallBlockState(2);
            }
        }
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
            bw.Write(EventExecuted);
            bw.Write(SuperStop);
            bw.Write(IsBeingPushed);
            bw.Write(PushAllowInertia);
            bw.Write(BlockStun);
            bw.Write(Uncrouched);
            bw.Write(PushGravity);
            bw.Write(HBounceIntensity);
            bw.Write(HBounceState);
            bw.Write(VBounceIntensity);
            bw.Write(VBounceState);
            bw.Write(HitstunType);
            bw.Write(GravityDecayFactor);
            bw.Write(HitstunDecayFactor);

            for (int i = 0; i < Spawnables.Length; ++i)
                for (int j = 0; j < Spawnables[i].Length; ++j)
                    Spawnables[i][j].Serialize(bw);
            
            for (int i = 0; i < VFX.Length; ++i)
                for (int j = 0; j < VFX[i].Length; ++j)
                    VFX[i][j].Serialize(bw);
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
            EventExecuted = br.ReadBoolean();
            SuperStop = br.ReadBoolean();
            IsBeingPushed = br.ReadBoolean();
            PushAllowInertia = br.ReadBoolean();
            BlockStun = br.ReadBoolean();
            Uncrouched = br.ReadBoolean();
            PushGravity = br.ReadInt32();
            HBounceIntensity = br.ReadInt32();
            HBounceState = br.ReadInt32();
            VBounceIntensity = br.ReadInt32();
            VBounceState = br.ReadInt32();
            HitstunType = br.ReadByte();
            GravityDecayFactor = br.ReadByte();
            HitstunDecayFactor = br.ReadByte();

            Body.UpdateColliders();

            for (int i = 0; i < Spawnables.Length; ++i)
                for (int j = 0; j < Spawnables[i].Length; ++j)
                    Spawnables[i][j].Deserialize(br);
            
            for (int i = 0; i < VFX.Length; ++i)
                for (int j = 0; j < VFX[i].Length; ++j)
                    VFX[i][j].Deserialize(br);
        }
#endregion
    }
}
