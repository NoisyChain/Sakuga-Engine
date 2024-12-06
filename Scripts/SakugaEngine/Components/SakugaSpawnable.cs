using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    [Icon("res://Sprites/Icons/Icon_Spawnable.png")]
    public partial class SakugaSpawnable : SakugaActor, IDamage
    {
        [ExportCategory("Timers")]
        [Export] public FrameTimer LifeTime;

        [ExportCategory("Variables")]
        [Export] public int InitialState;
        [Export] public int DeathState;
        [Export] public int DeflectState = -1;
        [Export] public bool DieOnGround;
        [Export] public bool DieOnWalls;
        [Export] public bool DieOnHit;
        [Export] public bool Deflectable;
        [Export] public bool CountLifeTimeOnSpawn = true;
        [Export] public Global.SpawnableHitCheck HitCheck;

        public bool IsActive;
        private byte CurrentHitCheck;

        private SakugaFighter _owner;

        public SakugaFighter GetFighterOwner() => _owner;
        public void SetFighterOwner(SakugaFighter owner) { _owner = owner; }
        public override SakugaFighter FighterReference() { return GetFighterOwner(); }

        public override bool AllowHitCheck(SakugaActor other)
        {
            if (CurrentHitCheck == 1 && other != _owner) return false;
            if (CurrentHitCheck == 0 && other != _owner.GetOpponent()) return false;

            return true;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            for (int i = 0; i < Graphics.Length; ++i)
                Graphics[i].Visible = IsActive;
        }

        protected override bool LifeEnded() { return !LifeTime.IsRunning(); }

        public void Initialize(SakugaFighter owner)
        {
            IsActive = false;
            CurrentHitCheck = (byte)HitCheck;
            SetFighterOwner(owner);
            Body.Initialize(this);
            Body.CurrentHitbox = -1;
            Body.IsLeftSide = GetFighterOwner().Body.IsLeftSide;
            Inputs = _owner.Inputs;
            Animator.PlayState(InitialState);
            Animator.Frame = -1;
        }

        public void Spawn(Vector2I origin)
        {
            CurrentHitCheck = (byte)HitCheck;
            Body.MoveTo(origin);
            Body.IsLeftSide = GetFighterOwner().Body.IsLeftSide;
            Animator.PlayState(InitialState);
            Animator.Frame = -1;
            LifeTime.Start();
            if (!CountLifeTimeOnSpawn) LifeTime.Pause();

            IsActive = true;
        }

        public void Reset()
        {
            IsActive = false;
            CurrentHitCheck = (byte)HitCheck;
            Body.IsLeftSide = GetFighterOwner().Body.IsLeftSide;
            Body.FixedVelocity = Vector2I.Zero;
            Body.FixedPosition = Vector2I.Zero;
            Animator.PlayState(InitialState);
            Animator.Frame = -1;
            Body.SetHitbox(-1);
        }

        public override void Tick()
        {
            if (!IsActive) return;

            Body.PlayerSide = Body.IsLeftSide ? 1 : -1;
            Body.IsMovable = !GetFighterOwner().HitStop.IsRunning();
            
            if (!GetFighterOwner().HitStop.IsRunning())
            {
                LifeTime.Run();
                Animator.RunState();
            }

            if (Variables != null) Variables.UpdateExtraVariables();
            CheckDeathConditions();
            
            if (!LifeTime.IsRunning() && Animator.Frame >= Animator.GetCurrentState().Duration - 1)
            {
                IsActive = false;
                Animator.Frame = -1;
                Body.SetHitbox(-1);
            }

            if (!GetFighterOwner().HitStop.IsRunning())
            {
                UpdateFrameProperties();
                StateTransitions();
                AnimationEvents();
                SoundEvents(SFXList, VoicesList);
            }
            UpdateHitboxes(!GetFighterOwner().HitStop.IsRunning());

            UpdateFighterPhysics();
        }

        private void CheckDeathConditions()
        {
            if ((DieOnWalls && Body.IsOnWall) || (DieOnGround && Body.IsOnGround) || !LifeTime.IsRunning())
            { LifeTime.Stop(); Animator.PlayState(DeathState); }
        }

        public void HitConfirm(int superGaugeGain, uint hitStopDuration, int hitConfirmAnimation, int hitEffect, Vector2I VFXSpawn)
        {
            GD.Print(hitEffect);
            GetFighterOwner().LayerSorting = 1;
            GetFighterOwner().Body.HitConfirmed = true;
            GetFighterOwner().Body.IsMovable = false;
            Body.HitConfirmed = true;
            Body.IsMovable = false;
            GetFighterOwner().Variables.AddSuperGauge(superGaugeGain);
            GetFighterOwner().HitStop.Start(hitStopDuration);
            if (hitEffect >= 0)
            {
                GetFighterOwner().SpawnVFX(hitEffect, VFXSpawn);
                //sounds.QueueSound(sounds.Last, hitEffect);
            }

            if (DieOnHit)
            { LifeTime.Stop(); Animator.PlayState(DeathState); }
            else if (hitConfirmAnimation >= 0)
                Animator.PlayState(hitConfirmAnimation, false);
            
            if (Variables != null) Variables.ExtraVariablesOnHit();
        }

        public void BaseDamage(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            GetFighterOwner().SetOpponent(target.FighterReference());
            SakugaFighter Opp = GetFighterOwner().GetOpponent();

            bool isHitAllowed = !Opp.Body.ContainsFrameProperty((byte)Global.FrameProperties.PROJECTILE_IMUNITY);
            if (!isHitAllowed) return;

            bool HitPosition = box.HitType == Global.HitType.UNBLOCKABLE || 
                                box.HitType == Global.HitType.HIGH && Opp.IsCrouching() || 
                                box.HitType == Global.HitType.LOW && !Opp.IsCrouching();
            if (Opp.Variables.SuperArmor > 0)
            {
                Opp.ArmorHit(box);
                HitConfirm(box.SelfMeterGain, (uint)box.ClashHitStopDuration, -1, box.ArmorHitEffectIndex, contact);
                GD.Print("Spawnable: Armor Hit");
            }
            else
            {
                int hitFX;
                if (Opp.IsBlocking() && !HitPosition)
                {
                    hitFX = box.BlockEffectIndex;
                    Opp.BlockHit(box);
                    GD.Print("Spawnable: Blocked!");
                }
                else
                {
                    hitFX = box.HitEffectIndex;
                    Opp.HitDamage(box);
                    GD.Print("Spawnable: Hit!");
                }
                HitConfirm(box.SelfMeterGain, (uint)box.HitStopDuration, box.HitConfirmState, hitFX, contact);
                //if (box.AllowSelfPushback)
                    //HitPushback(box.SelfPushbackDuration, box.SelfPushbackForce);
            }
        }
        public void ThrowDamage(SakugaActor target, HitboxElement box, Vector2I contact){}
        public void HitboxClash(HitboxElement box, Vector2I contact){}
        public void ProjectileClash(HitboxElement box, Vector2I contact)
        {
            if (DieOnHit)
            { LifeTime.Stop(); Animator.PlayState(DeathState); }
        }
        public void ProjectileDeflect(SakugaActor target, HitboxElement box, Vector2I contact)
        {
            if (!Deflectable) return;

            if (CurrentHitCheck != 2)
                if (CurrentHitCheck == 1)
                    CurrentHitCheck = 0;
                else
                    CurrentHitCheck = 1;
        
            LifeTime.Start();

            if (DeflectState < 0)
                Animator.PlayState(Animator.CurrentState, true);
            else Animator.PlayState(DeflectState, true);
        
            Body.IsLeftSide = !Body.IsLeftSide;
            //Body.PlayerSide = Body.IsLeftSide ? 1 : -1;
        }
        public void CounterHit(SakugaActor target, HitboxElement box, Vector2I contact){}
        public void ParryHit(SakugaActor target, HitboxElement box, Vector2I contact){}
        public void ProximityBlock(){}
        public void OnHitboxExit(){}

        public override void Serialize(BinaryWriter bw)
        {
            bw.Write(IsActive);
            bw.Write(CurrentHitCheck);
            Body.Serialize(bw);
            if (Variables != null) Variables.Serialize(bw);
            Animator.Serialize(bw);
            LifeTime.Serialize(bw);
        }
		public override void Deserialize(BinaryReader br)
        {
            IsActive = br.ReadBoolean();
            CurrentHitCheck = br.ReadByte();
            Body.Deserialize(br);
            if (Variables != null) Variables.Deserialize(br);
            Animator.Deserialize(br);
            LifeTime.Deserialize(br);

            Body.UpdateColliders();
        }
    }
}
