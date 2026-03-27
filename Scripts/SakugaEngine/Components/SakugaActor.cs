using System.Linq;
using Godot;
using SakugaEngine.Collision;
using SakugaEngine.Global;
using SakugaEngine.Resources;
using SakugaEngine.GameState;
using MessagePack;

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
        public bool SuperFlashing;
        public bool CinematicState;
        public bool BlockStun;
        public HitstunType HitstunType;
        public FrameProperties FrameProperties = 0;
        public HitChecker HitCheck;
        public CancelCondition CancelConditions;

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

        public bool ContainsFrameProperty(Global.FrameProperties CompareTo)
        {
            return (FrameProperties & CompareTo) != 0;
        }

        public int InputSide => GetMaster() != null ? GetMaster().Body.PlayerSide : Body.PlayerSide;

#region Conditions
        public bool IsGroundState() => Body != null && Body.IsOnGround && StateManager.GetCurrentState().BaseStance == MasterStance.NEUTRAL;
        public bool IsCrouchState() => Body != null && Body.IsOnGround && StateManager.GetCurrentState().BaseStance == MasterStance.CROUCH;
        public bool IsAirState() => Body != null && !Body.IsOnGround && StateManager.GetCurrentState().BaseStance == MasterStance.NEUTRAL;
        public bool IsKO() => Parameters != null && Parameters.Health != null && Parameters.Health.CurrentValue <= 0;
        public bool IsStunLocked() => OnHitstun() && StateManager.GetCurrentState().AnimationData != null && Hitstun.TimeLeft >= StateManager.GetCurrentState().AnimationData.Duration - StateManager.GetCurrentState().AnimationData.HitstunHold + 1 && StateManager.CurrentStateFrame >= StateManager.GetCurrentState().AnimationData.HitstunHold;
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

        public StanceSelect SelectBlockStance()
        {
            if (IsGroundState()) return StanceSelect.GROUND;
            else if (IsCrouchState()) return StanceSelect.CROUCH;
            else return StanceSelect.AIR;
        }
#endregion

        public override void Initialize()
        {
            base.Initialize();
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
            if (Pool != null) Pool.Initialize(this);

            FrameProperties = InitialProperties;
            HitCheck = AllowHit;
        }

        public void InitializeBrain(bool active, BotDifficulty diff)
        {
            if (Brain == null) return;

            UseAI = active;
            Brain.Initialize(this, diff);
        }

        public void Reset()
        {
            if (Body != null)
            {
                Body.FixedPosition.X = GlobalVariables.StartingPosition * (-1 + ((int)playerID * 2));
                Body.UpdateSide(Mathf.Sign(Body.FixedPosition.X) < 0);
                Body.FlipSide();
            }

            if (StanceManager != null) StanceManager.CallStance(0);
            if (StateManager != null) StateManager.Reset();
            if (Parameters != null) Parameters.Reset();
            //if (Pool != null) Pool.Initialize(this);

            FrameProperties = InitialProperties;
            HitCheck = AllowHit;
        }

        public override void PreTick()
        {
            if (!IsActive) return;

            if (CanHitstop()) Hitstop.Run();
            if (Body != null) Body.IsMovable = !OnHitstop();
            
            if (OnHitstop()) return;

            if (CanHitstun() && HitstunType < HitstunType.HARD_KNOCKDOWN)
                Hitstun.Run();

            if (Parameters != null)
                Parameters.Reset(!OnHitstun());

            if (Parameters != null) Parameters.Tick();
            if (StateManager != null && !IsStunLocked()) StateManager.RunState();

            foreach(FrameDataEvent transitionEvent in StateManager.GetCurrentState().TransitionEvents)
                transitionEvent.RunEvent(ref actor);
        }

        public override void Tick()
        {
            if (!IsActive) return;

            if (StanceManager != null) StanceManager.CheckMoves();

            if (OnHitstop()) return;

            foreach(FrameDataEvent tickEvent in StateManager.GetCurrentState().OnTickEvents)
                tickEvent.RunEvent(ref actor);
        }

        public override void LateTick()
        {
            if (!IsActive) return;
            if (OnHitstop()) return;
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
            if (Animator != null) Animator.ViewAnimations(StateManager.GetCurrentAnimationSettings(), StateManager.CurrentStateFrame);
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
#region Hit logic
        public void HitDamage(SakugaActor target, HitboxElement box, bool isTrade)
        {
            // Change hit fighter layering
            LayerSorting = -1;
            // Select hit values
            bool isGroundHit = !IsKO() && Body != null && IsGroundState() && !IsAirState();
            var finalHitstun = isGroundHit ? box.GroundHitstun : box.AirHitstun;
            var finalKnockbackTime = isGroundHit ? box.GroundHitKnockbackTime : box.AirHitKnockbackTime;
            var finalDamageKnockback = isGroundHit ? box.GroundHitKnockback : box.AirHitKnockback;
            var finalKnockbackGravity = isGroundHit ? box.GroundHitKnockbackGravity : box.AirHitKnockbackGravity;
            var finalHitReaction = isGroundHit ? (IsCrouchState() ? box.CrouchHitReaction : box.GroundHitReaction) : box.AirHitReaction;
            HitstunType = isGroundHit ? box.GroundHitstunType : box.AirHitstunType;
            bool CanTech = StateManager != null && StateManager.CurrentStateType() == StateType.HIT_REACTION && !OnHitstun();
            
            if (Body != null)
            {
                Body.UpdateSide(!target.Body.IsLeftSide);
                Body.FlipSide();
                Body.IsMovable = false;
            }

            int finalHitstunForReal = Mathf.Max(finalHitstun, GlobalVariables.MinHitstun);
            int decayingKnockback = 0;
            if (Parameters != null)
            {
                Parameters.ChangeVariablesBehavior(CustomVariableBehaviorTarget.ON_DAMAGE);
                finalHitstunForReal = Mathf.Max(finalHitstun - Parameters.Prorations.HitstunDecayFactor, GlobalVariables.MinHitstun);
                decayingKnockback = GlobalVariables.GravityDecay * Parameters.Prorations.GravityDecayFactor;
            }

            if (CanHitstop())
            {
                if (!isTrade)
                    Hitstop.Start((uint)box.OpponentHitStopDuration);
                else 
                    Hitstop.Stop();
            }

            if (CanHitstun()) Hitstun.Start((uint)finalHitstunForReal);

            foreach(FrameDataEvent hitEvent in StateManager.GetCurrentState().OnHitReactionEvents)
                hitEvent.RunEvent(ref actor);

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
                int finalDamage = Parameters.CalculateProrations(
                    box.BaseDamage, finalAttack,
                    Parameters.Prorations.CurrentDefense,
                    (ushort)box.DamageScalingSubtract
                );
                Parameters.TakeDamage(
                    finalDamage,
                    box.OpponentSuperGaugeGain,
                    box.KillingBlow
                );
                
                Parameters.Tracker.UpdateTrackers(finalDamage, stunFrame, finalHitstunForReal, (int)box.HitType, CanTech);
                if (target.Parameters.Tracker != null) target.Parameters.Tracker.FrameAdvantage = -(stunFrame - finalHitstunForReal);
                
                if (Parameters.Tracker.HitCombo >= GlobalVariables.HitstunDecayMinCombo)
                    Parameters.Prorations.HitstunDecayFactor++;
                if (IsAirState())
                    Parameters.Prorations.GravityDecayFactor++;
            }
            if (CanBounce())
            {
                Bounce.Start((uint)box.BounceTime);
                BounceYIntensity = box.BounceXIntensity;
                BounceYIntensity = box.BounceYIntensity;
            }
        }

        public void BlockHit(SakugaActor target, HitboxElement box, int blockState) 
        {
            LayerSorting = -1;
            bool isGroundHit = !IsKO() && Body != null && IsGroundState() && !IsAirState();
            var finalHitstun = isGroundHit ? box.GroundBlockstun : box.AirBlockstun;
            var finalKnockbackTime = isGroundHit ? box.GroundBlockKnockbackTime : box.AirBlockKnockbackTime;
            var finalDamageKnockback = isGroundHit ? box.GroundBlockKnockback : box.AirBlockKnockback;
            var finalKnockbackGravity = isGroundHit ? box.GroundBlockKnockbackGravity : box.AirBlockKnockbackGravity;

            BlockStun = true;

            if (Body != null)
            {
                Body.UpdateSide(!target.Body.IsLeftSide);
                Body.FlipSide();
                Body.IsMovable = false;
            }
            
            if (StanceManager != null) StanceManager.Clear();
            if (StateManager != null) StateManager.PlayState(blockState, true);

            if (CanHitstun()) Hitstun.Start((uint)finalHitstun);
            if (CanHitstop()) Hitstop.Start((uint)box.BlockStopDuration);
            
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
                    box.ChipDamage, finalAttack,
                    Parameters.Prorations.CurrentDefense,
                    0
                );
                Parameters.TakeDamage(
                    finalDamage,
                    box.OpponentSuperGaugeGain,
                    box.ChipDeath
                );

                Parameters.Tracker.UpdateTrackers(finalDamage, stunFrame, finalHitstun, (int)box.HitType, false);
            }
        }
        public void GuardCrush(SakugaActor target, HitboxElement box, int crushState)
        {
            if (Body != null)
            {
                Body.UpdateSide(!target.Body.IsLeftSide);
                Body.FlipSide();
                Body.IsMovable = false;
            }
            if (StanceManager != null) StanceManager.Clear();
            if (StateManager != null) StateManager.PlayState(crushState, true);

            if (CanHitstun()) Hitstun.Start(GlobalVariables.GuardCrushHitstun);
            if (CanHitstop()) Hitstop.Start((uint)box.BlockStopDuration);

            if (Parameters != null)
            {
                int stunFrame = target.StateManager.GetCurrentState().AnimationData.Duration - target.StateManager.CurrentStateFrame;
                int finalAttack = target.Parameters.Prorations != null ? target.Parameters.Prorations.CurrentAttack : target.Data.BaseAttack;
                int finalDamage = Parameters.CalculateProrations(
                    box.GuardCrushDamage, finalAttack,
                    Parameters.Prorations.CurrentDefense,
                    (ushort)box.DamageScalingSubtract
                );
                Parameters.TakeDamage(
                    finalDamage,
                    box.OpponentSuperGaugeGain,
                    box.KillingBlow
                );

                Parameters.Tracker.UpdateTrackers(finalDamage, stunFrame, GlobalVariables.GuardCrushHitstun, (int)box.HitType, false);
            }
        }
        public void ArmorHit(SakugaActor target, HitboxElement box)
        {
            //Armor hit
            LayerSorting = -1;
            HitConfirm(target, box.OpponentSuperGaugeGain, (uint)box.ClashHitStopDuration, -1, Vector2I.Zero);
            if (Parameters != null)
            {
                int finalAttack = target.Parameters.Prorations != null ? target.Parameters.Prorations.CurrentAttack : target.Data.BaseAttack;
                int finalDamage = Parameters.CalculateProrations(
                    box.BaseDamage, finalAttack,
                    Parameters.Prorations.CurrentDefense,
                    (ushort)box.DamageScalingSubtract
                );

                Parameters.ArmorDamage(box.ArmorDamage, finalDamage / 2);
            }
        }
        public void ThrowHit(SakugaActor target, HitboxElement box, uint finalHitStop)
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
                        int finalHitReaction = Mathf.Clamp(box.ThrowHitReaction, 0, StanceManager.GetCurrentStance().HitReactions.Length - 1);
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
            if (Inputs.IsBeingPressed(Inputs.CurrentHistory, PlayerInputs.ANY_BUTTON))
            {
                HitstunType = 0;
                ThrowEscapeAction();
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
            PushActor(10, -40000 * Body.PlayerSide, 0, 0);
        }
        public void HitConfirm(SakugaActor target, int superGaugeGain, uint hitStopDuration, int hitEffect, Vector2I VFXSpawn)
        {
            LayerSorting = 1;

            if (Body != null) Body.IsMovable = false;
            
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
                if (Parameters.SuperGauge != null) Parameters.SuperGauge.AddSuperGauge(superGaugeGain);
                Parameters.ChangeVariablesBehavior(CustomVariableBehaviorTarget.ON_HIT);
            }

            if (GetMaster() != null && GetMaster().Parameters.SuperGauge != null)
                GetMaster().Parameters.SuperGauge.AddSuperGauge(superGaugeGain);

            foreach(FrameDataEvent hitEvent in StateManager.GetCurrentState().OnHitConfirmEvents)
                hitEvent.RunEvent(ref actor);

            if (Body != null) Body.AddHitBody(target.Body);
        }
        public void BaseDamage(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            bool isHitAllowed = !target.ContainsFrameProperty(Global.FrameProperties.DAMAGE_IMUNITY);
            if (!isHitAllowed) return;

            bool HitPosition = box.HitType == HitType.UNBLOCKABLE || 
                                box.HitType == HitType.HIGH && target.Body.IsOnGround && target.IsCrouchState() || 
                                box.HitType == HitType.LOW && target.Body.IsOnGround && target.IsGroundState() || target.IsAirState();

            CancelConditions &= ~CancelCondition.WHIFF_CANCEL;
            CancelConditions &= ~CancelCondition.KARA_CANCEL;

            int blockState = target.StanceManager.GetBlockState(target.SelectBlockStance(), box.HitType, (byte)(box.GuardCrush ? 2 : 1));

            if (target.Parameters.SuperArmor != null && target.Parameters.SuperArmor.CurrentValue > 0)
            {
                target.ArmorHit(actor, box);
                HitConfirm(target, box.SelfSuperGaugeGain, (uint)box.ClashHitStopDuration, box.ArmorEffectIndex, contact);
                CancelConditions |= CancelCondition.HIT_CANCEL;
                GD.Print("Fighter: Armor Hit");
            }
            else
            {
                int hitFX;
                if (target.CanBlock() && !HitPosition && blockState < 0)
                {
                    if (box.GuardCrush)
                    {
                        hitFX = box.GuardCrushEffectIndex;
                        target.GuardCrush(actor, box, blockState);
                        GD.Print("Fighter: Guard Crushed!");
                        CancelConditions |= CancelCondition.HIT_CANCEL;
                    }
                    else
                    {
                        hitFX = box.BlockEffectIndex;
                        target.BlockHit(actor, box, blockState);
                        if (box.AllowSelfPushback)
                            HitPushback(target, box.SelfPushbackDuration, box.SelfPushbackForce);
                        GD.Print("Fighter: Blocked!");
                    }
                    CancelConditions |= CancelCondition.BLOCK_CANCEL;
                }
                else
                {
                    hitFX = box.HitEffectIndex;
                    target.HitDamage(actor, box, false);
                    if (box.AllowSelfPushback)
                        HitPushback(target, box.SelfPushbackDuration, box.SelfPushbackForce);
                    CancelConditions |= CancelCondition.HIT_CANCEL;
                    GD.Print("Fighter: Hit!");
                }
                //if (Brain != null && UseAI) Brain.canAdvance = true;
                //if (target.Brain != null && target.UseAI) target.Brain.Reset();
                HitConfirm(target, box.SelfSuperGaugeGain, (uint)box.SelfHitStopDuration, hitFX, contact);
            }
        }
        public void HitTrade(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            HitDamage(target, box, true);
            if (box.HitEffectIndex >= 0 && contact != Vector2I.Zero)
            {
                if (Pool != null)
                    Pool.SpawnVFX(box.HitEffectIndex, contact, Body.PlayerSide, false);
                else if (GetMaster().Pool != null)
                    GetMaster().Pool.SpawnVFX(box.HitEffectIndex, contact, Body.PlayerSide, false);
            }
            GD.Print("Fighter: Traded!");
        }
        public void ThrowDamage(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            bool isThrowAllowed = !target.ContainsFrameProperty(Global.FrameProperties.THROW_IMUNITY) && 
                                    ((target.Body.IsOnGround && box.GroundThrow) || 
                                    (target.IsAirState() && box.AirThrow) ||
                                    (box.GroundThrow && box.AirThrow));
            if (!isThrowAllowed) return;

            uint finalHitstop = (uint)box.ThrowHitstop;
            if (target.StateManager.CurrentStateType() == StateType.HIT_REACTION) finalHitstop = (uint)box.ThrowHitstopAfterHit;
            
            target.ThrowHit(target, box, finalHitstop);
            HitConfirm(target, 0, finalHitstop, -1, Vector2I.Zero);
            //if (Brain != null && UseAI) Brain.canAdvance = true;
            GD.Print("Fighter: Throw!");
        }
        public void ThrowTrade()
        {
            ThrowEscapeAction();
        }
        public void HitboxClash(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            HitConfirm(target, 0, (uint)box.ClashHitStopDuration, box.ClashEffectIndex, contact);
            //if (Brain != null && UseAI) Brain.canAdvance = true;
        }
        public void ProjectileClash(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            HitConfirm(target, 0, 0, box.ClashEffectIndex, contact);
            //if (Brain != null && UseAI) Brain.canAdvance = true;
        }
        public void ProjectileDeflect(SakugaActor target, HitboxElement box, Vector2I contact){}
        public void CounterHit(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            HitConfirm(target, box.SelfSuperGaugeGain, (uint)box.SelfHitStopDuration, box.HitEffectIndex, contact);
            target.HitConfirm(target, box.OpponentSuperGaugeGain, (uint)box.OpponentHitStopDuration, -1, Vector2I.Zero);
            //if (Brain != null && UseAI) Brain.canAdvance = true;
        }
        public void ProximityBlock(HitboxElement box)
        {
            if (StanceManager == null) return;
            if (StateManager == null) return;
            int blockState = StanceManager.GetBlockState(SelectBlockStance(), box.HitType, 0);
            if (blockState < 0) return;

            if (UseAI && Brain != null) Brain.proximityHit = box.HitType;
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
            

            if (!BlockStun && StateManager.CurrentStateType() == StateType.BLOCKING)
            {
                int blockState = StanceManager.ExitBlockState(SelectBlockStance());
                if (blockState < 0) return;
                StateManager.PlayState(blockState, true);
            }
        }
#endregion

        public string DebugInfo()
        {
            return "Position: "+Body.FixedPosition+
                    "\nVelocity: "+Body.FixedVelocity+
                    "\nStance: "+StanceManager.CurrentStance+
                    "\nCurrent State: "+StateManager.CurrentState+
                    "\nState Name: "+StateManager.GetCurrentState().StateName+
                    "\nAnimation Name: "+(StateManager.GetCurrentAnimationSettings() == null ? "" : StateManager.GetCurrentAnimationSettings().SourceAnimation)+
                    "\nCurrent Move: "+StanceManager.CurrentMove+
                    "\nBuffered Move: "+StanceManager.BufferedMove+
                    "\nFrame: "+StateManager.CurrentStateFrame+
                    "\nHitstun Type: "+HitstunType+
                    "\nHitbox: "+Body.CurrentHitbox+
                    "\nHealth: "+Parameters.Health.CurrentValue+"/"+Data.MaxHealth+
                    "\nSuper Gauge: "+Parameters.SuperGauge.CurrentValue+"/"+Data.MaxSuperGauge+
                    "\nSuper Armor: "+Parameters.SuperArmor.CurrentValue+
                    "\nHit Stop: ("+ Hitstop.IsRunning()+") "+Hitstop.TimeLeft+
                    "\nHit Stun: ("+ Hitstun.IsRunning()+") "+Hitstun.TimeLeft+
                    "\nMove Buffer: ("+ StanceManager.MoveBuffer.IsRunning()+") "+StanceManager.MoveBuffer.TimeLeft+
                    "\nCharge Buffers: "+Inputs.hCharge+" | "+Inputs.vCharge;//+
                    //"\nBlocking: "+IsBlocking();
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
            if (Data.Intros == null || Data.Intros.Length == 0) return;

            if (Data.Intros.Length == 1)
            {
                StateManager.PlayState(Data.Intros[0].StateIndex);
                return;
            }

            FighterIntro introToPlay = null;

            for (int i = 0; i < Data.VictoryPoses.Length; i++)
            {
                if (Data.VictoryPoses[i].ForOpponent == opponentName)
                {
                    introToPlay = Data.VictoryPoses[i];
                    break;
                }
                else if (Data.VictoryPoses[i].ForOpponent == "") introToPlay = Data.VictoryPoses[i];
            }

            StateManager.PlayState(introToPlay.StateIndex);
        }
        public void PlayOutro(string opponentName, out string VictoryMessage)
        {
            VictoryMessage = $"{Data.Profile.ShortName} is winner!";
            if (Data.Outros == null || Data.Outros.Length == 0) return;

            if (Data.Outros.Length == 1)
            {
                StateManager.PlayState(Data.Outros[0].StateIndex);
                VictoryMessage = Data.Outros[0].FinalMessage;
                return;
            }

            FighterOutro outroToPlay = null;

            for (int i = 0; i < Data.Outros.Length; i++)
            {
                if (Data.Outros[i].ForOpponent == opponentName)
                {
                    outroToPlay = Data.Outros[i];
                    break;
                }
                else if (Data.Outros[i].ForOpponent == "") outroToPlay = Data.Outros[i];
            }

            StateManager.PlayState(outroToPlay.StateIndex);
            VictoryMessage = outroToPlay.FinalMessage;
        }

        public void PlayVictoryPose(string opponentName)
        {
            if (Data.VictoryPoses == null || Data.VictoryPoses.Length == 0) return;

            if (Data.VictoryPoses.Length == 1)
            {
                StateManager.PlayState(Data.VictoryPoses[0].StateIndex);
                return;
            }

            FighterIntro introToPlay = null;

            for (int i = 0; i < Data.VictoryPoses.Length; i++)
            {
                if (Data.VictoryPoses[i].ForOpponent == opponentName)
                {
                    introToPlay = Data.VictoryPoses[i];
                    break;
                }
                else if (Data.VictoryPoses[i].ForOpponent == "") introToPlay = Data.VictoryPoses[i];
            }

            StateManager.PlayState(introToPlay.StateIndex);
        }

        public void PlayDefeatPose()
        {
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
