using System.Linq;
using Godot;
using SakugaEngine.Collision;
using SakugaEngine.Global;
using SakugaEngine.Resources;
using SakugaEngine.GameState;
using MessagePack;
using SakugaEngine.Game;

namespace SakugaEngine
{
    [GlobalClass]
	public partial class SakugaActor : SakugaNode, IDamage
    {
        public InputManager Inputs;
		[ExportCategory("Components")]
		[Export] public PhysicsBody Body;
		[Export] public SakugaParameters Parameters;
		[Export] public StateManager StateManager;
		[Export] public StanceManager StanceManager;
        [Export] public ObjectPool Pool;
        [Export] public AIBrain Brain;
        [ExportCategory("Variables")]
        [Export] public FrameProperties InitialProperties;
        [Export] public HitChecker AllowHit;
		[ExportCategory("Visuals")]
        [Export] private AnimationViewer Animator;
        [Export] protected Node3D[] Graphics;
        [ExportCategory("Resources")]
        [Export] public DataContainer Data;
        [Export] public SpawnsList SpawnablesList;
        [Export] public SpawnsList VFXList;
        [Export] public SoundsList SFXList;
        [Export] public SoundsList VoiceLines;
        [ExportCategory("Default Timers")]
        [Export] public FrameTimer Hitstop;
        [Export] public FrameTimer Hitstun;
        [Export] public FrameTimer Knockback;
        [Export] public FrameTimer Bounce;

        // Internal variables
        public bool UseAI;
        public uint playerID;
        public int LayerSorting = -1;
        public int BounceXIntensity = 0;
        public int BounceYIntensity = 0;
        public bool CinematicState;
        public HitstunType HitstunType;
        public FrameProperties FrameProperties = 0;
        public HitChecker HitCheck;
        public CancelCondition CancelConditions;
        public int CurrentGrabbedNodeID = -1;

        // References
        private SakugaActor actor;
        private SakugaActor _master;
        private SakugaActor[] _allies;
        private SakugaActor[] _opponents;
        private SakugaActorState State;

        public SakugaActor GetMaster() => _master;
        public void SetMaster(SakugaActor master) { if (master != _master) _master = master; }

        public SakugaActor[] GetAllies() => GetMaster() != null ? GetMaster().GetAllies() : _allies;
        public SakugaActor GetAlly(int index) => GetMaster() != null ? GetMaster().GetAlly(index) : _allies[index];
        public void SetAllies(SakugaActor[] allies) { if (allies != _allies) _allies = allies; }

        public SakugaActor[] GetOpponents() => GetMaster() != null ? GetMaster().GetOpponents() : _opponents;
        public SakugaActor GetOpponent(int index) => GetMaster() != null ? GetMaster().GetOpponent(index) : _opponents[index];
        public void SetOpponents(SakugaActor[] opponents) { if (opponents != _opponents) _opponents = opponents; }

        public bool IsCounter(SakugaActor target)
		{
			return StateManager.CurrentStateType() == StateType.COMBAT && target.StateManager.CurrentStateType() == StateType.COMBAT &&
				 	Body.CurrentHitbox.animationStage < AnimationStage.RECOVERY &&
                    target.Body.CurrentHitbox.animationStage < AnimationStage.RECOVERY;
		} 
		public bool IsPunishCounter(SakugaActor target)
        {
			return StateManager.CurrentStateType() == StateType.COMBAT && target.StateManager.CurrentStateType() == StateType.COMBAT &&
				 	Body.CurrentHitbox.animationStage < AnimationStage.RECOVERY &&
                    target.Body.CurrentHitbox.animationStage == AnimationStage.RECOVERY;
		}
        public bool IsInstantBlock()
        {
            return StateManager.CurrentStateType() != StateType.BLOCKING;
        }
        public bool CanHitTarget(SakugaActor target)
        {
            // Ignore if either one of the bodies is not active
            if (!IsActive || !target.IsActive) return false;
            if (HitCheck == 0) return false;
            // Ignore if both have the same master
            if (GetMaster() != null && target.GetMaster() != null && target.GetMaster() == GetMaster()) return false;
            // Ignore if target is master
            if ((HitCheck & HitChecker.MASTER) > 0 && GetMaster() != null && GetMaster() != target) return false;
            // Ignore if you can't hit opponents
            if ((HitCheck & HitChecker.OPPONENTS) > 0 && !GetOpponents().Contains(target) && !GetOpponents().Contains(target.GetMaster())) return false;
            // Ignore if you can't hit allies
            if ((HitCheck & HitChecker.ALLIES) > 0 && !GetAllies().Contains(target) && !GetAllies().Contains(target.GetMaster())) return false;

            GD.Print("Hit is valid");
            return true;
        }

        public SakugaActor GetClosestAlly()
        {
            if (_allies.Length == 1) return _allies[0];
             
            int dist = int.MaxValue;
            int index = 0;

            for(int i = 0; i < _allies.Length; i++)
            {
                int newDist = GlobalFunctions.HorizontalDistance(Body.FixedPosition, _allies[i].Body.FixedPosition);
                if (newDist < dist)
                {
                    dist = newDist;
                    index = i;
                }
            }

            return _allies[index];
        }

        public SakugaActor GetClosestOpponent()
        {
            if (_opponents.Length == 1) return _opponents[0];
             
            int dist = int.MaxValue;
            int index = 0;

            for(int i = 0; i < _opponents.Length; i++)
            {
                int newDist = GlobalFunctions.HorizontalDistance(Body.FixedPosition, _opponents[i].Body.FixedPosition);
                if (newDist < dist)
                {
                    dist = newDist;
                    index = i;
                }
            }

            return _opponents[index];
        }

#region Conditions
        public bool IsGroundState() => Body != null && Body.IsOnGround && StateManager.GetCurrentState().BaseStance == MasterStance.NEUTRAL;
        public bool IsCrouchState() => Body != null && Body.IsOnGround && StateManager.GetCurrentState().BaseStance == MasterStance.CROUCH;
        public bool IsAirState() => Body != null && (!Body.IsOnGround || Body.IsOnGround && StateManager.GetCurrentState().BaseStance == MasterStance.ON_THE_GROUND);
        public bool IsKO() => Parameters != null && Parameters.Health != null && Parameters.Health.CurrentValue <= 0;
        public bool IsStunLocked() => OnHitstun() && StateManager.GetCurrentState().AnimationData != null && StateManager.GetCurrentState().AnimationData.HitstunHold >= 0 && Hitstun.TimeLeft >= StateManager.GetCurrentState().AnimationData.Duration - StateManager.GetCurrentState().AnimationData.HitstunHold + 1 && StateManager.CurrentStateFrame >= StateManager.GetCurrentState().AnimationData.HitstunHold;
        public bool CanBlock() => StanceManager != null && StanceManager.GetCurrentStance().BlockReactions != null && StanceManager.GetCurrentStance().BlockReactions.Length > 0;
        public bool IsGrabbed() => HitstunType == HitstunType.GRABBED;
        public bool CanHitstop() => Hitstop != null;
        public bool OnHitstop() => CanHitstop() && Hitstop.IsRunning();
        public bool CanHitstun() => Hitstun != null;
        public bool OnHitstun() => CanHitstun() && Hitstun.IsRunning();
        public bool CanKnockback() => Body != null && Knockback != null;
        public bool OnKnockback() => CanKnockback() && Knockback.IsRunning();
        public bool CanBounce() => Bounce != null;
        public bool IsBouncingX() => CanBounce() && Bounce.IsRunning() && BounceXIntensity > 0;
        public bool IsBouncingY() => CanBounce() && Bounce.IsRunning() && BounceYIntensity > 0;
        public bool ContainsFrameProperty(FrameProperties CompareTo) => (FrameProperties & CompareTo) != 0;

        public int InputSide(InputSideCheck sideCheck = InputSideCheck.CHARACTER_RELATIVE)
        {
            switch (sideCheck)
            {
                case InputSideCheck.ABSOLUTE:
                    return 1;
                case InputSideCheck.SIDE_RELATIVE:
                    return Body.IsLeftSide ? 1 : -1;
                case InputSideCheck.CHARACTER_RELATIVE:
                    return Body.PlayerSide;
            }
            return 1;
        }

        public StanceSelect SelectBlockStance()
        {
            if (IsGroundState()) return StanceSelect.GROUND;
            else if (IsCrouchState()) return StanceSelect.CROUCH;
            else return StanceSelect.AIR;
        }
#endregion

        public override void Initialize(uint id)
        {
            base.Initialize(id);
            actor = this;
            if (Body != null)
            {
                Body.Initialize(this, this);
                Body.FixedPosition.X = GlobalVariables.StartingPosition * (-1 + ((int)playerID * 2));
                Body.UpdateSide(Mathf.Sign(Body.FixedPosition.X) < 0);
                Body.FlipSide();
            }
            if (StanceManager != null) StanceManager.Initialize(this);
            if (StateManager != null) StateManager.Initialize(this);
            if (Parameters != null) Parameters.Initialize(this);

            FrameProperties = InitialProperties;
            HitCheck = AllowHit;
        }
        
        public void InitializePool()
        {
            if (Pool != null) Pool.Initialize(this);
        }

        public void InitializeBrain(bool active, BotDifficulty diff)
        {
            if (Brain == null) return;

            UseAI = active;
            Brain.Initialize(this, diff);
        }

        public void Reset()
        {
            IsActive = StartActive;
            if (Body != null)
            {
                Body.FixedPosition.X = GlobalVariables.StartingPosition * (-1 + ((int)playerID * 2));
                Body.FixedPosition.Y = 0;
                Body.SetVelocity(Vector2I.Zero);
                Body.UpdateSide(Mathf.Sign(Body.FixedPosition.X) < 0);
                Body.FlipSide();
                Body.IsMovable = true;
            }

            if (StanceManager != null) StanceManager.CallStance(0);
            if (StateManager != null) StateManager.Reset();
            if (Parameters != null) Parameters.Reset();
            if (Pool != null) Pool.Reset();

            FrameProperties = InitialProperties;
            HitCheck = AllowHit;
        }

        public override void PreTick()
        {
            if (!IsActive) return;

            if (CanHitstop()) Hitstop.Run();
            if (Body != null) Body.IsMovable = !OnHitstop() && !IsSuperFlashing();
            
            if (OnHitstop() || IsSuperFlashing()) return;

            if (CanHitstun() && HitstunType < HitstunType.HARD_KNOCKDOWN)
                Hitstun.Run();

            if (StateManager.CurrentStateType() < StateType.BLOCKING || StateManager.CurrentStateType() == StateType.LOCKED)
            {
                if (CanHitstun()) Hitstun.Stop();
                HitstunType = HitstunType.NONE;
                if (Parameters != null) Parameters.Clear();
            }

            if (Parameters != null) Parameters.Tick();
            if (StateManager != null && !IsStunLocked()) StateManager.RunState();

            StateManager.GetCurrentState().RunTransitionEvents(ref actor);
        }

        public override void Tick()
        {
            if (!IsActive) return;

            if (CurrentGrabbedNodeID >= 0)
                ThrowEscape(GameManager.Instance.GetActor(CurrentGrabbedNodeID) as SakugaActor);
            if (StanceManager != null) StanceManager.CheckMoves();

            if (OnHitstop() || IsSuperFlashing()) return;

            StateManager.GetCurrentState().RunOnTickEvents(ref actor);
        }

        public override void LateTick()
        {
            if (!IsActive) return;
            if (OnHitstop() || IsSuperFlashing()) return;
        }

        public override void Render()
        {
            foreach (Node3D g in Graphics)
            {
                g.Visible = IsActive;
                g.GlobalPosition = GlobalFunctions.ToScaledVector3(Body.FixedPosition);
                g.GlobalRotation = Vector3.Zero;
                g.Scale = new Vector3(Body.PlayerSide, 1, 1);
            }
            if (Animator != null)
            {
                Animator.SetDepth(LayerSorting);
                Animator.ViewAnimations(StateManager.GetCurrentAnimationSettings(), StateManager.CurrentStateFrame);
            }
        }
#region Knockback physics (not sure if it's gonna stay here)
        public void PushActor(int pushDuration, int VelocityX, int VelocityY, int gravity)
        {
            Body.FixedVelocity.X = VelocityX;
            Body.FixedVelocity.Y = VelocityY;
            Body.CurrentGravity = gravity;
            Knockback.Start((uint)pushDuration);
        }

        public void HitPushback(SakugaActor target, int duration, int velX)
        {
            if (velX == 0) return;
            
            int pushbackSide = Body.FixedPosition.X > 0 ? 1 : -1;
            if (Body.IsOnGround)
                if (target.Body.IsOnWall)
                    PushActor(duration, velX * pushbackSide, 0, 0);
        }

        public void StopKnockback(bool keepXVelocity)
        {
            if (!keepXVelocity) Body.FixedVelocity.X = 0;
            Knockback.Stop();
        }
#endregion
        private bool ValidTechState()
        {
            bool validTech;
            switch (HitstunType)
            {
                case HitstunType.BASIC:
                    validTech = true;
                    break;
                case HitstunType.KNOCKDOWN:
                    validTech = Body != null && Body.IsOnGround;
                    break;
                default:
                    validTech = false;
                    break;
            }
            
            return validTech;
        }

        public bool IsSuperFlashing() => GameManager.Instance.IsSuperFlashing && !ContainsFrameProperty(FrameProperties.SUPER_FLASH_IMMUNITY);

        public void StartSuperFlash(ushort duration, bool immune)
        {
            if (immune) FrameProperties |= FrameProperties.SUPER_FLASH_IMMUNITY;
            GameManager.Instance.SetSuperFlash(duration);
        }

        public void StopSuperFlash()
        {
            if (ContainsFrameProperty(FrameProperties.SUPER_FLASH_IMMUNITY))
                FrameProperties &= ~FrameProperties.SUPER_FLASH_IMMUNITY;
            GameManager.Instance.SetSuperFlash(0);
        }
#region Hit logic
        public void HitDamage(SakugaActor target, AttackProfile profile, bool isTrade)
        {
            // Change hit fighter layering
            LayerSorting = -1;
            // Select hit values
            bool isGroundHit = !IsKO() && Body != null && !IsAirState();
            var finalHitstun = isGroundHit ? profile.CurrentGroundHitstun : profile.CurrentAirHitstun;
            var finalKnockbackTime = profile.CurrentHitKnockbackDuration;
            var finalDamageKnockback = isGroundHit ? profile.GroundHitKnockback : profile.AirHitKnockback;
            var finalKnockbackGravity = isGroundHit ? profile.CurrentGroundKnockbackGravity : profile.CurrentAirKnockbackGravity;
            var finalHitReaction = isGroundHit ? (IsCrouchState() ? profile.CrouchHitReaction : profile.GroundHitReaction) : profile.AirHitReaction;
            HitstunType = isGroundHit ? profile.GroundHitstunType : profile.AirHitstunType;
            bool canTech = StateManager != null && StateManager.CurrentStateType() == StateType.HIT_REACTION && !OnHitstun() && ValidTechState();
            
            if (Body != null)
            {
                Body.UpdateSide(!target.Body.IsLeftSide);
                Body.FlipSide();
                Body.IsMovable = false;
            }

            //int finalHitstunForReal = Mathf.Max((int)finalHitstun, (int)GlobalVariables.MinHitstun);
            int decayingKnockback = 0;
            if (Parameters != null)
            {
                //finalHitstunForReal = Mathf.Max((int)finalHitstun - Parameters.Prorations.HitstunDecayFactor, (int)GlobalVariables.MinHitstun);
                finalHitstun -= Parameters.Prorations.HitstunDecayFactor;
                decayingKnockback = GlobalVariables.GravityDecay * Parameters.Prorations.GravityDecayFactor;
            }

            int finalHitstunForReal = Mathf.Max((int)finalHitstun, (int)GlobalVariables.MinHitstun);

            if (CanHitstop())
            {
                if (!isTrade)
                    Hitstop.Start(profile.CurrentOpponentHitstop);
                else 
                    Hitstop.Stop();
            }

            if (CanHitstun()) Hitstun.Start((uint)finalHitstunForReal);

            StateManager.GetCurrentState().RunOnHitReactionEvents(ref actor);

            if (StanceManager != null)
            {
                StanceManager.Clear();
                if (StanceManager.GetCurrentStance().HitReactions != null && StanceManager.GetCurrentStance().HitReactions.Length >= 0)
                {
                    finalHitReaction = Mathf.Clamp(finalHitReaction, 0, StanceManager.GetCurrentStance().HitReactions.Length - 1);
                    if (StateManager != null) 
                        StateManager.PlayState(StanceManager.GetCurrentStance().HitReactions[finalHitReaction], true);
                }
            }
            
            PushActor(
                finalKnockbackTime, 
                finalDamageKnockback.X * (-target.Body.PlayerSide),
                finalDamageKnockback.Y - decayingKnockback, 
                finalKnockbackGravity
            );

            if (Parameters != null)
            {
                int stunFrame = target.StateManager == null ? 0 : target.StateManager.GetCurrentState().AnimationData.Duration - target.StateManager.CurrentStateFrame;
                int finalAttack = target.Parameters.Prorations != null ? target.Parameters.Prorations.CurrentAttack : target.Data.BaseAttack;
                ushort finalDamageScaling = profile.CurrentDamageScaling;
                if (!profile.MultiHitDamageScaling && target.Body.HitConfirmed) finalDamageScaling = 0;
                int finalDamage = Parameters.CalculateProrations(
                    profile.BaseDamage, finalAttack,
                    Parameters.Prorations.CurrentDefense,
                    finalDamageScaling
                );
                Parameters.TakeDamage(
                    finalDamage,
                    profile.CurrentSuperGainOpp,
                    profile.KillingBlow
                );
                
                Parameters.Tracker.UpdateTrackers(finalDamage, stunFrame, finalHitstunForReal, (int)profile.HitType, canTech);

                if (target.Parameters.Tracker != null)
                    if (isTrade)
                        Parameters.Tracker.FrameAdvantage = -finalHitstunForReal;
                    else
                        target.Parameters.Tracker.FrameAdvantage = -(stunFrame - finalHitstunForReal);
                
                if (Parameters.Tracker.HitCombo >= GlobalVariables.HitstunDecayMinCombo)
                    Parameters.Prorations.HitstunDecayFactor++;
                if (IsAirState())
                    Parameters.Prorations.GravityDecayFactor++;
            }

            StartBounce((uint)profile.BounceTime, profile.BounceXIntensity, profile.BounceYIntensity);
        }

        public void BlockHit(SakugaActor target, AttackProfile profile, int blockState) 
        {
            LayerSorting = -1;
            bool isGroundHit = !IsKO() && Body != null && IsGroundState() && !IsAirState();
            var finalHitstun = isGroundHit ? profile.CurrentGroundBlockstun : profile.CurrentAirBlockstun;
            var finalKnockbackTime = profile.CurrentBlockKnockbackDuration;
            var finalDamageKnockback = isGroundHit ? profile.GroundBlockKnockback : profile.AirBlockKnockback;
            var finalKnockbackGravity = isGroundHit ? 0 : GlobalVariables.DefaultGravity;

            if (Body != null)
            {
                Body.UpdateSide(!target.Body.IsLeftSide);
                Body.FlipSide();
                Body.IsMovable = false;
            }

            if (IsInstantBlock())
            {
                GameManager.Instance.PlayHitNotificationByIndex((int)playerID, 3);
            }
            
            if (StanceManager != null) StanceManager.Clear();
            if (StateManager != null) StateManager.PlayState(blockState, true);

            if (CanHitstun()) Hitstun.Start(finalHitstun);
            if (CanHitstop()) Hitstop.Start(profile.CurrentBlockstop);
            
            PushActor(
                finalKnockbackTime,
                finalDamageKnockback.X * (-target.Body.PlayerSide),
                finalDamageKnockback.X,
                finalKnockbackGravity
            );
            if (Parameters != null)
            {
                int stunFrame = target.StateManager == null ? 0 : target.StateManager.GetCurrentState().AnimationData.Duration - target.StateManager.CurrentStateFrame;
                int finalAttack = target.Parameters.Prorations != null ? target.Parameters.Prorations.CurrentAttack : target.Data.BaseAttack;
                int finalDamage = Parameters.CalculateProrations(
                    profile.ChipDamage, finalAttack,
                    Parameters.Prorations.CurrentDefense,
                    0
                );
                Parameters.TakeDamage(
                    finalDamage,
                    profile.CurrentSuperGainOpp,
                    profile.ChipDeath
                );

                Parameters.Tracker.UpdateTrackers(finalDamage, stunFrame, (int)finalHitstun, (int)profile.HitType, false, false);
                
                if (target.Parameters.Tracker != null)
                    target.Parameters.Tracker.FrameAdvantage = -(stunFrame - (int)finalHitstun);
            }
        }
        public void GuardCrush(SakugaActor target, AttackProfile profile, int crushState)
        {
            bool isGroundHit = !IsKO() && Body != null && IsGroundState() && !IsAirState();
            var finalKnockbackTime = profile.CurrentBlockKnockbackDuration;
            var finalDamageKnockback = isGroundHit ? profile.GroundBlockKnockback : profile.AirBlockKnockback;
            var finalKnockbackGravity = isGroundHit ? 0 : GlobalVariables.DefaultGravity;

            if (Body != null)
            {
                Body.UpdateSide(!target.Body.IsLeftSide);
                Body.FlipSide();
                Body.IsMovable = false;
            }
            if (StanceManager != null) StanceManager.Clear();
            if (StateManager != null) StateManager.PlayState(crushState, true);

            if (CanHitstun()) Hitstun.Start(GlobalVariables.GuardCrushHitstun);
            if (CanHitstop()) Hitstop.Start(GlobalVariables.DefaultBaseHitstop);

            PushActor(
                finalKnockbackTime,
                finalDamageKnockback.X * (-target.Body.PlayerSide),
                finalDamageKnockback.X,
                finalKnockbackGravity
            );

            if (Parameters != null)
            {
                int stunFrame = target.StateManager.GetCurrentState().AnimationData.Duration - target.StateManager.CurrentStateFrame;
                int finalAttack = target.Parameters.Prorations != null ? target.Parameters.Prorations.CurrentAttack : target.Data.BaseAttack;
                int finalDamage = Parameters.CalculateProrations(
                    profile.GuardCrushDamage, finalAttack,
                    Parameters.Prorations.CurrentDefense,
                    profile.CurrentDamageScaling
                );
                Parameters.TakeDamage(
                    finalDamage,
                    GlobalVariables.DefaultOpponentSuperGaugeGain,
                    profile.KillingBlow
                );

                Parameters.Tracker.UpdateTrackers(finalDamage, stunFrame, (int)GlobalVariables.GuardCrushHitstun, (int)profile.HitType, false);
            }
        }
        public void ArmorHit(SakugaActor target, AttackProfile profile)
        {
            //Armor hit
            LayerSorting = -1;
            HitConfirm(target, profile.CurrentSuperGainOpp, GlobalVariables.DefaultClashHitstop, -1, Vector2I.Zero);
            Body.ResetHits();
            if (Parameters != null)
            {
                int finalAttack = target.Parameters.Prorations != null ? target.Parameters.Prorations.CurrentAttack : target.Data.BaseAttack;
                int finalDamage = Parameters.CalculateProrations(
                    profile.BaseDamage, finalAttack,
                    Parameters.Prorations.CurrentDefense,
                    profile.CurrentDamageScaling
                );

                Parameters.ArmorDamage(profile.CurrentArmorDamage, finalDamage / 2);
            }
        }
        public void ThrowHit(SakugaActor target, AttackProfile profile, uint finalHitStop)
        {
            LayerSorting = -1;
            HitstunType = HitstunType.GRABBED;
            if (Parameters.SuperArmor != null) Parameters.SuperArmor.SetArmor(0);
            if (Body != null)
            {
                Body.UpdateSide(!target.Body.IsLeftSide);
                Body.FlipSide();
                Body.IsMovable = false;
            }
            
            if (CanHitstop()) Hitstop.Start(finalHitStop);

            if (StanceManager != null)
            {
                StanceManager.Clear();
                if (StanceManager.GetCurrentStance().HitReactions != null && StanceManager.GetCurrentStance().HitReactions.Length >= 0)
                {
                    if (StateManager != null)
                    {
                        int finalHitReaction = Mathf.Clamp(profile.ThrowHitReaction, 0, StanceManager.GetCurrentStance().HitReactions.Length - 1);
                        StateManager.PlayState(StanceManager.GetCurrentStance().HitReactions[finalHitReaction], true);
                    }
                }
            }

            if (Parameters != null)
                Parameters.ChangeVariablesBehavior(CustomVariableBehaviorTarget.ON_HIT);
        }
        public void ThrowEscape(SakugaActor target)
        {
            if (!Hitstop.IsRunning()) return;
            if (HitstunType < HitstunType.GRABBED) return;
            if (Inputs.CheckMotionInputs(StanceManager.GetCurrentStance().ThrowEscapeInput, InputSide(InputSideCheck.SIDE_RELATIVE)))
            {
                HitstunType = 0;
                ThrowEscapeAction();
                GameManager.Instance.PlayHitNotificationByIndex((int)playerID, 4);
                target.ThrowEscapeAction();
            }
        }
        public void ThrowEscapeAction()
        {
            if (StanceManager == null) return;
            int selfSelectThrowEscape = Body.IsOnGround ? 
                                        StanceManager.GetCurrentStance().GroundThrowEscapeState : 
                                        StanceManager.GetCurrentStance().AirThrowEscapeState;
            if (CanHitstop()) Hitstop.Stop();
            if (StateManager != null) StateManager.PlayState(selfSelectThrowEscape);
            CurrentGrabbedNodeID = -1;
            PushActor(10, -40000 * Body.PlayerSide, 0, 0);
        }
        public void HitConfirm(SakugaActor target, int superGaugeGain, uint hitStopDuration, int hitEffect, Vector2I VFXSpawn)
        {
            LayerSorting = 1;

            if (Body != null)
            {
                Body.IsMovable = false;
                Body.HitConfirmed = true;
            }
            
            if (CanHitstop()) Hitstop.Start(hitStopDuration);

            if (hitEffect >= 0 && VFXSpawn != Vector2I.Zero)
            {
                if (Pool != null)
                    Pool.SpawnVFX(hitEffect, VFXSpawn, Body.PlayerSide, false);
                else if (GetMaster().Pool != null)
                    GetMaster().Pool.SpawnVFX(hitEffect, VFXSpawn, Body.PlayerSide, false);
            }
            
            if (Parameters != null)
            {
                Parameters.HitConfirm(superGaugeGain);
            }

            if (GetMaster() != null && GetMaster().Parameters.SuperGauge != null)
                GetMaster().Parameters.SuperGauge.AddSuperGauge(superGaugeGain);
            
            if (Body != null) Body.AddHitBody(target.Body);

            StateManager.GetCurrentState().RunOnHitConfirmEvents(ref actor);
        }
        public void BaseDamage(SakugaActor target, AttackProfile profile, Vector2I contact)
        {
            bool isHitAllowed = !target.ContainsFrameProperty(FrameProperties.DAMAGE_IMMUNITY);
            if (!isHitAllowed) return;

            CurrentGrabbedNodeID = -1;
            target.CurrentGrabbedNodeID = -1;

            bool HitPosition = profile.HitType == HitType.UNBLOCKABLE || 
                                profile.HitType == HitType.HIGH && target.Body.IsOnGround && target.IsCrouchState() || 
                                profile.HitType == HitType.LOW && (target.Body.IsOnGround && target.IsGroundState() || target.IsAirState());

            CancelConditions &= ~CancelCondition.WHIFF_CANCEL;
            CancelConditions &= ~CancelCondition.KARA_CANCEL;

            int blockState = target.StanceManager.GetBlockState(profile.HitType, (byte)(profile.GuardCrush ? 2 : 1));
            GD.Print(blockState);

            if (target.Parameters.SuperArmor != null && target.Parameters.SuperArmor.CurrentValue > 0)
            {
                target.ArmorHit(actor, profile);
                HitConfirm(target, profile.CurrentSuperGainOpp, GlobalVariables.DefaultArmorHitstop, GlobalVariables.DefaultArmorVFX, contact);
                CancelConditions |= CancelCondition.HIT_CANCEL;
                GD.Print("Fighter: Armor Hit");
            }
            else
            {
                if (target.CanBlock() && blockState >= 0)
                {
                    if (profile.GuardCrush)
                    {
                        target.GuardCrush(actor, profile, blockState);
                        GameManager.Instance.PlayHitNotificationByIndex((int)playerID, 10);
                        GD.Print("Fighter: Guard Crushed!");
                        CancelConditions |= CancelCondition.HIT_CANCEL;
                    }
                    else
                    {
                        target.BlockHit(actor, profile, blockState);
                        HitPushback(target, GlobalVariables.DefaultKnockbackDuration, profile.CurrentCornerPushback);
                        GD.Print("Fighter: Blocked!");
                    }
                    CancelConditions |= CancelCondition.BLOCK_CANCEL;
                    HitConfirm(
                        target, profile.CurrentSuperGainSelf, profile.CurrentBlockstop, 
                        profile.GuardCrush ? GlobalVariables.DefaultGuardCrushVFX : GlobalVariables.DefaultBlockVFX, 
                        contact
                    );
                }
                else
                {
                    bool isCounter = IsCounter(target);
                    if (GameManager.Instance.WaitingForFirstStrike())
                        GameManager.Instance.PlayHitNotificationByIndex((int)playerID, 0);

                    if (isCounter)
                        GameManager.Instance.PlayHitNotificationByIndex((int)playerID, 1);
                    else if (IsPunishCounter(target))
                        GameManager.Instance.PlayHitNotificationByIndex((int)playerID, 2);

                    target.HitDamage(actor, profile, false);
                    HitPushback(target, GlobalVariables.DefaultKnockbackDuration, profile.CurrentCornerPushback);
                    CancelConditions |= CancelCondition.HIT_CANCEL;
                    GD.Print("Fighter: Hit!");

                    HitConfirm(target, profile.CurrentSuperGainSelf, profile.CurrentSeftHitstop, profile.CurrentHitVFX, contact);

                    if (isCounter)
                    {
                        Hitstop.Start(GlobalVariables.DefaultCounterHitstop);
                        target.Hitstop.Start(GlobalVariables.DefaultCounterHitstop);
                    }
                }
                //if (Brain != null && UseAI) Brain.canAdvance = true;
                //if (target.Brain != null && target.UseAI) target.Brain.Reset();
                
            }

            bool targetKOed = target.Parameters.Health != null && target.Parameters.Health.CurrentValue <= 0;
            bool selfKOed = Parameters.Health != null && Parameters.Health.CurrentValue <= 0;

            if (targetKOed || selfKOed)
            {
                Hitstop.Stop();
                target.Hitstop.Stop();
                StartSuperFlash((ushort)GlobalVariables.FinalBlowHitstop, false);
            }
        }
        public void HitTrade(SakugaActor target, AttackProfile profile, Vector2I contact)
        {
            GameManager.Instance.PlayHitNotificationByIndex((int)playerID, 1);
            HitDamage(target, profile, true);
            if (contact != Vector2I.Zero)
            {
                if (Pool != null)
                    Pool.SpawnVFX(GlobalVariables.DefaultHitVFX, contact, Body.PlayerSide, false);
                else if (GetMaster().Pool != null)
                    GetMaster().Pool.SpawnVFX(GlobalVariables.DefaultHitVFX, contact, Body.PlayerSide, false);
            }
            GD.Print("Fighter: Traded!");
        }
        public void ThrowDamage(SakugaActor target, AttackProfile profile, Vector2I contact)
        {
            bool isThrowAllowed = !target.ContainsFrameProperty(FrameProperties.THROW_IMMUNITY) && 
                                    ((target.Body.IsOnGround && profile.GroundThrow) || 
                                    (target.IsAirState() && profile.AirThrow) ||
                                    (profile.GroundThrow && profile.AirThrow));
            if (!isThrowAllowed) return;
            
            uint finalHitstop = GlobalVariables.DefaultThrowHitstop;
            if (target.StateManager.CurrentStateType() == StateType.HIT_REACTION) finalHitstop = GlobalVariables.DefaultLongThrowHitstop;
            if (profile.Immediate) finalHitstop = 0;
            
            CurrentGrabbedNodeID = (int)target.NodeID;
            target.CurrentGrabbedNodeID = (int)NodeID;

            target.ThrowHit(actor, profile, finalHitstop);
            HitConfirm(target, 0, finalHitstop, -1, Vector2I.Zero);
            //if (Brain != null && UseAI) Brain.canAdvance = true;
            GD.Print("Fighter: Throw!");
        }
        public void ThrowTrade()
        {
            ThrowEscapeAction();
        }
        public void HitboxClash(SakugaActor target, AttackProfile profile, Vector2I contact)
        {
            HitConfirm(target, 0, GlobalVariables.DefaultClashHitstop, GlobalVariables.DefaultClashVFX, contact);
            CancelConditions |= CancelCondition.HIT_CANCEL;
            //if (Brain != null && UseAI) Brain.canAdvance = true;
        }
        public void ProjectileClash(SakugaActor target, AttackProfile profile, Vector2I contact)
        {
            HitConfirm(target, 0, 0, GlobalVariables.DefaultClashVFX, contact);
            //if (Brain != null && UseAI) Brain.canAdvance = true;
        }
        public void ProjectileDeflect(SakugaActor target, AttackProfile profile, Vector2I contact){}
        public void CounterHit(SakugaActor target, AttackProfile profile, Vector2I contact)
        {
            HitConfirm(target, profile.CurrentSuperGainSelf, profile.CurrentSeftHitstop, profile.CurrentHitVFX, contact);
            target.HitConfirm(target, profile.CurrentSuperGainOpp, profile.CurrentOpponentHitstop, -1, Vector2I.Zero);
            //if (Brain != null && UseAI) Brain.canAdvance = true;
        }
        public void ProximityBlock(HitType type = HitType.MEDIUM)
        {
            if (StanceManager == null) return;
            if (StateManager == null) return;
            int blockState = StanceManager.GetBlockState(type, 0);
            if (blockState < 0) return;

            if (UseAI && Brain != null) Brain.proximityHit = type;
            if (Body != null) Body.ProximityBlocked = true;

            if (CanBlock() && StateManager.CurrentStateType() != StateType.BLOCKING)
            {
                StateManager.PlayState(blockState, true);
                StanceManager.Clear();
            }
        }
        public void OnHitboxExit()
        {
            if (StanceManager == null) return;
            if (StateManager == null) return;
            
            if (!OnHitstun() && StateManager.CurrentStateType() == StateType.BLOCKING)
            {
                int blockState = StanceManager.ExitBlockState(SelectBlockStance());
                if (blockState < 0) return;
                StateManager.PlayState(blockState, true);
            }
        }
#endregion

        public string DebugInfo()
        {
            return "Match State: "+GameManager.Instance.Monitor.MatchState+
                    "\nPosition: "+Body.FixedPosition+
                    "\nVelocity: "+Body.FixedVelocity+
                    "\nStance: "+StanceManager.CurrentStance+
                    "\nCurrent State: "+StateManager.CurrentState+
                    "\nState Name: "+StateManager.GetCurrentState().StateName+
                    "\nAnimation Index: "+StateManager.CurrentAnimationSettings+
                    "\nAnimation Name: "+(StateManager.GetCurrentAnimationSettings() == null ? "None" : StateManager.GetCurrentAnimationSettings().SourceAnimation)+
                    "\nCurrent Move: "+StanceManager.CurrentMove+
                    "\nBuffered Move: "+StanceManager.BufferedMove+
                    "\nFrame: "+StateManager.CurrentStateFrame+
                    "\nHitstun Type: "+HitstunType+
                    "\nHitbox Index: "+Body.CurrentHitboxSettings+
                    "\nHealth: "+Parameters.Health.CurrentValue+"/"+Data.MaxHealth+
                    "\nRecov. Health: "+Parameters.Health.RecoverableValue+
                    "\nSuper Gauge: "+Parameters.SuperGauge.CurrentValue+"/"+Data.MaxSuperGauge+
                    "\nBurst Gauge: "+Parameters.BurstGauge.CurrentValue+"/"+Data.MaxBurst+
                    "\nSuper Armor: "+Parameters.SuperArmor.CurrentValue+
                    "\nInput Side: "+InputSide()+
                    "\nFrame Properties: "+FrameProperties+
                    "\nHit Stop: ("+ Hitstop.IsRunning()+") "+Hitstop.TimeLeft+
                    "\nHit Stun: ("+ Hitstun.IsRunning()+") "+Hitstun.TimeLeft+
                    "\nMove Buffer: ("+ StanceManager.MoveBuffer.IsRunning()+") "+StanceManager.MoveBuffer.TimeLeft+
                    "\nBounce: ("+Bounce.TimeLeft+", X:"+BounceXIntensity+", Y:"+BounceYIntensity+")"+
                    "\nCharge Buffers: "+Inputs.hCharge+" | "+Inputs.vCharge+
                    "\nHit Confirmed: "+(Body != null && Body.HitBodies.Count > 0)+
                    "\nCurrently Grab Actor: "+CurrentGrabbedNodeID;
        }

        public void StartBounce(uint bounceTime, int bounceX = 100, int bounceY = 100)
        {
            if (!CanBounce()) return;

            if (bounceTime == 0)
            {
                Bounce.Stop();
                BounceXIntensity = 0;
                BounceYIntensity = 0;
                return;
            }

            Bounce.Start(bounceTime);
            BounceXIntensity = bounceX;
            BounceYIntensity = bounceY;
        }

        public Vector2I GenerateTargetPosition(Vector2I Target, int index, RelativeTo xRelative, RelativeTo yRelative)
        {
            int relativePosX = 0;
            int relativePosY = 0;
            switch (xRelative)
            {
                case RelativeTo.WORLD:
                    break;
                case RelativeTo.SELF:
                    relativePosX = Body.FixedPosition.X;
                    Target.X *= Body.PlayerSide;
                    break;
                case RelativeTo.OPPONENT:
                    relativePosX = GetOpponents()[0].Body.FixedPosition.X;
                    Target.X *= GetOpponents()[0].Body.PlayerSide;
                    break;
                case RelativeTo.FIGHTER: //By index
                    switch (index)
                    {
                        case 0:
                            relativePosX = Body.FixedPosition.X;
                            Target.X *= Body.PlayerSide;
                            break;
                        case 1:
                            relativePosX = GetOpponents()[0].Body.FixedPosition.X;
                            Target.X *= GetOpponents()[0].Body.PlayerSide;
                            break;
                    }
                    break;
                case RelativeTo.SPAWNABLE: //By index
                    relativePosX = Pool.GetActiveSpawnByIndex(index).Body.FixedPosition.X;
                    Target.X *= Pool.GetActiveSpawnByIndex(index).Body.PlayerSide;
                    break;
            }

            switch (yRelative)
            {
                case RelativeTo.WORLD:
                    break;
                case RelativeTo.SELF:
                    relativePosY = Body.FixedPosition.Y;
                    break;
                case RelativeTo.OPPONENT:
                    relativePosY = GetOpponents()[0].Body.FixedPosition.Y;
                    break;
                case RelativeTo.FIGHTER: //By index
                    switch (index)
                    {
                        case 0:
                            relativePosY = Body.FixedPosition.Y;
                            break;
                        case 1:
                            relativePosY = GetOpponents()[0].Body.FixedPosition.Y;
                            break;
                    }
                    break;
                case RelativeTo.SPAWNABLE: //By index
                    relativePosY = Pool.GetActiveSpawnByIndex(index).Body.FixedPosition.Y;
                    break;
            }

            Vector2I finalPosition = new Vector2I(Target.X + relativePosX, Target.Y + relativePosY);
            return finalPosition;
        }

        public void PlayIntro(string opponentName)
        {
            var intro = Data.GetIntro(opponentName);
            if (!GlobalFunctions.Assert(intro != null, "This fighter doesn't contain intro states. Skipping...")) return;

            StateManager.PlayState(intro.StateIndex);
        }
        public void PlayOutro(string opponentName, out string VictoryMessage)
        {
            var outro = Data.GetOutro(opponentName, out VictoryMessage);
            if (!GlobalFunctions.Assert(outro != null, "This fighter doesn't contain outro states. Skipping...")) return;

            StateManager.PlayState(outro.StateIndex);
        }

        public void PlayVictoryPose(string opponentName)
        {
            var victory = Data.GetVictoryPose(opponentName);
            if (!GlobalFunctions.Assert(victory != null, "This fighter doesn't contain victory poses. Skipping...")) return;

            StateManager.PlayState(victory.StateIndex);
        }

        public void PlayDefeatPose()
        {
            if (!GlobalFunctions.Assert(Data.DefeatPose >= 0, "This fighter doesn't contain defeat poses. Skipping...")) return;
            StateManager.PlayState(Data.DefeatPose);
        }

        public override byte[] GetStateData()
        {
            State.GetStateData(ref actor);
            return MessagePackSerializer.Serialize(State);
        }
		public override void SetStateData(byte[] stateBuffer)
        {
            State = MessagePackSerializer.Deserialize<SakugaActorState>(stateBuffer);
            State.SetStateData(ref actor);

            if (Body != null) Body.UpdateColliders();
        }
	}
}
