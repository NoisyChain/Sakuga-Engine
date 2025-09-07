using Godot;
using SakugaEngine.Collision;
using SakugaEngine.Resources;

namespace SakugaEngine
{
	public partial class SakugaActor : SakugaNode
	{
		[ExportCategory("Components")]
		[Export] public PhysicsBody Body;
		[Export] public InputManager Inputs;
		[Export] public SakugaVariables Variables;
		[Export] public FrameAnimator Animator;
		[Export] public StanceManager Stance;
		[Export] public CombatTracker Tracker;
        [Export] public SoundQueue[] Sounds;

		[ExportCategory("Visuals")]
        [Export] protected Node3D[] Graphics;

        [ExportCategory("Lists")]
        [Export] public SpawnsList VFXList;
        [Export] public SoundsList SFXList;
        [Export] public SoundsList VoicesList;

        protected bool EventExecuted;

        public virtual SakugaFighter FighterReference() { return null; }

        public virtual bool LifeEnded() { return false; }
        
        public override void Render()
        {
            GlobalPosition = Global.ToScaledVector3(Body.FixedPosition);
            foreach (Node3D g in Graphics)
                g.Scale = new Vector3(Body.PlayerSide, 1, 1);
            Animator.ViewAnimations();
        }

		public void UpdateFighterPhysics()
        {
            if (Animator.GetCurrentState().statePhysics.Length == 0) return;

            for (int i = 0; i < Animator.GetCurrentState().statePhysics.Length; ++i)
            {
                int nextFrame = i + 1 < Animator.GetCurrentState().statePhysics.Length ?
                                        Animator.GetCurrentState().statePhysics[i + 1].Frame :
                                        Animator.GetCurrentState().Duration;
                if (Animator.Frame >= Animator.GetCurrentState().statePhysics[i].Frame && Animator.Frame < nextFrame)
                {
                    if (Animator.GetCurrentState().statePhysics[i].UseLateralSpeed)
                    {
                        int InputSide = Animator.GetCurrentState().statePhysics[i].UseHorizontalInput ? 0 : Inputs.InputSide;
                        if (Animator.GetCurrentState().statePhysics[i].UseHorizontalInput)
                            if (Inputs.IsBeingPressed(Inputs.CurrentHistory, Global.INPUT_RIGHT))
                                InputSide = 1;
                            else if (Inputs.IsBeingPressed(Inputs.CurrentHistory, Global.INPUT_LEFT))
                                InputSide = -1;

                        if (Animator.GetCurrentState().statePhysics[i].UseLateralInertia)
                            Body.AddLateralAcceleration(Animator.GetCurrentState().statePhysics[i].LateralSpeed * InputSide);
                        else
                            Body.SetLateralVelocity(Animator.GetCurrentState().statePhysics[i].LateralSpeed * InputSide);
                    }
                    if (Animator.GetCurrentState().statePhysics[i].UseVerticalSpeed)
                    {
                        int InputSide = Animator.GetCurrentState().statePhysics[i].UseHorizontalInput ? 0 : 1;
                        if (Animator.GetCurrentState().statePhysics[i].UseHorizontalInput)
                            if (Inputs.IsBeingPressed(Inputs.CurrentHistory, Global.INPUT_UP))
                                InputSide = 1;
                            else if (Inputs.IsBeingPressed(Inputs.CurrentHistory, Global.INPUT_DOWN))
                                InputSide = -1;

                        if (Animator.GetCurrentState().statePhysics[i].UseVerticalInertia)
                            Body.AddVerticalAcceleration(Animator.GetCurrentState().statePhysics[i].VerticalSpeed * InputSide);
                        else
                            Body.SetVerticalVelocity(Animator.GetCurrentState().statePhysics[i].VerticalSpeed * InputSide);
                    }
                    if (Animator.GetCurrentState().statePhysics[i].UseGravity)
                        Body.AddGravity(Animator.GetCurrentState().statePhysics[i].Gravity);
                }
            }
        }

		public void UpdateHitboxes()
        {
            for (int i = 0; i < Animator.GetCurrentState().hitboxStates.Length; ++i)
            {
                if (Animator.Frame == Animator.GetCurrentState().hitboxStates[i].Frame &&
                    Body.CurrentHitbox != Animator.GetCurrentState().hitboxStates[i].HitboxIndex)
                {
                    Body.SetHitbox(Animator.GetCurrentState().hitboxStates[i].HitboxIndex);
                    Body.HitConfirmed = false;
                }
            }
        }

        public void UpdateFrameProperties()
        {
            if (Animator.GetCurrentState().stateProperties.Length <= 0)
                Body.FrameProperties = 0;
            
            for (int i = 0; i < Animator.GetCurrentState().stateProperties.Length; ++i)
            {
                if (Animator.Frame == Animator.GetCurrentState().stateProperties[i].Frame)
                {
                    Body.FrameProperties = (byte)Animator.GetCurrentState().stateProperties[i].Properties;
                }
            }
        }

		public void StateTransitions()
        {
            if (Animator.GetCurrentState().stateTransitions.Length <= 0) return;

            for (int i = 0; i < Animator.GetCurrentState().stateTransitions.Length; i++)
            {
                if (Animator.GetCurrentState().stateTransitions[i].StateIndex < 0) continue;
                
                byte conditions = (byte)Animator.GetCurrentState().stateTransitions[i].Condition;
                if (conditions == 0) return;
                
                //Condition 1: State End
                bool c1 = (conditions & (byte)Global.TransitionCondition.STATE_END) == 0 ||
                        ((conditions & (byte)Global.TransitionCondition.STATE_END) != 0 && 
                        Animator.Frame >= Animator.GetCurrentState().Duration - 1);
                //Condition 2: At Frame
                bool c2 = (conditions & (byte)Global.TransitionCondition.AT_FRAME) == 0 ||
                    ((conditions & (byte)Global.TransitionCondition.AT_FRAME) != 0 && 
                    Animator.Frame == Animator.GetCurrentState().stateTransitions[i].AtFrame);
                //Condition 3: On Ground
                bool c3 = (conditions & (byte)Global.TransitionCondition.ON_GROUND) == 0 ||
                    ((conditions & (byte)Global.TransitionCondition.ON_GROUND) != 0 && Body.IsOnGround && Body.FixedVelocity.Y <= 0);
                //Condition 4: On Walls
                bool c4 = (conditions & (byte)Global.TransitionCondition.ON_WALLS) == 0 ||
                    ((conditions & (byte)Global.TransitionCondition.ON_WALLS) != 0 && Body.IsOnWall);
                //Condition 5: On Fall
                bool c5 = (conditions & (byte)Global.TransitionCondition.ON_FALL) == 0 ||
                    ((conditions & (byte)Global.TransitionCondition.ON_FALL) != 0 && Body.IsFalling);
                //Condition 6: On Life End
                bool c6 = (conditions & (byte)Global.TransitionCondition.ON_LIFE_END) == 0 ||
                    ((conditions & (byte)Global.TransitionCondition.ON_LIFE_END) != 0 && LifeEnded());
                //Condition 7: On Input Command
                bool c7 = (conditions & (byte)Global.TransitionCondition.ON_INPUT_COMMAND) == 0 ||
                    ((conditions & (byte)Global.TransitionCondition.ON_INPUT_COMMAND) != 0 && 
                    Inputs.CheckMotionInputs(Animator.GetCurrentState().stateTransitions[i].Inputs));
                //Condition 8: On Distance
                int distance = Global.Distance(FighterReference().GetOpponent().Body.FixedPosition, Body.FixedPosition).X;
                bool c8 = (conditions & (byte)Global.TransitionCondition.ON_DISTANCE) == 0 ||
                    ((conditions & (byte)Global.TransitionCondition.ON_DISTANCE) != 0 && 
                    distance >= Animator.GetCurrentState().stateTransitions[i].DistanceArea.X &&
                    distance <= Animator.GetCurrentState().stateTransitions[i].DistanceArea.Y);

                bool ValidTransition = c1 && c2 && c3 && c4 && c5 && c6 && c7 && c8;
                if (ValidTransition) Animator.PlayState(Animator.GetCurrentState().stateTransitions[i].StateIndex);
            }
        }

        public void AnimationEvents()
        {
            if (Animator.GetCurrentState().animationEvents.Length <= 0) return;
            if (EventExecuted) return;

            for (int i = 0; i < Animator.GetCurrentState().animationEvents.Length; i++)
            {
                if (Animator.Frame != Animator.GetCurrentState().animationEvents[i].Frame) continue;
                for (int j = 0; j < Animator.GetCurrentState().animationEvents[i].Events.Length; j++)
                {
                    switch (Animator.GetCurrentState().animationEvents[i].Events[j])
                    {
                        case SpawnObjectAnimationEvent spawnEvent: //Spawn Object (Spawnable, VFX)
                            Vector2I dst = Teleport(spawnEvent.TargetPosition, spawnEvent.Index,
                                    spawnEvent.xRelativeTo, spawnEvent.yRelativeTo);
                            
                            int ind = spawnEvent.IsRandom ? 
                                Global.RNG.Next(spawnEvent.Index, spawnEvent.Range) : 
                                spawnEvent.Index;
                            if (spawnEvent.FromExtraVariable >= 0)
                            {
                                ind = Variables.ExtraVariables[spawnEvent.FromExtraVariable].CurrentValue;
                                Variables.ExtraVariables[spawnEvent.FromExtraVariable].ChangeOnUse();
                            }
                            switch (spawnEvent.Object)
                                {
                                    case Global.ObjectType.SPAWNABLE:
                                        FighterReference().SpawnSpawnable(ind, dst);
                                        break;
                                    case Global.ObjectType.VFX:
                                        FighterReference().SpawnVFX(ind, dst, spawnEvent.FollowParent ? Body : null);
                                        break;
                                }
                            break;
                        case TeleportAnimationEvent teleportEvent: //Teleport
                            dst = Teleport(teleportEvent.TargetPosition, teleportEvent.Index,
                                    teleportEvent.xRelativeTo, teleportEvent.yRelativeTo);
                            
                            Body.MoveTo(dst);
                            break;
                        case ApplyDamageAnimationEvent damageEvent: //Animation Damage
                            AnimationDamage(damageEvent);
                            break;
                        case ChangeSideAnimationEvent changeSideEvent: //Force Side Change
                            switch (changeSideEvent.Index)
                            {
                                case 0:
                                    FighterReference().ForcePlayerSide(!FighterReference().GetOpponent().Body.IsLeftSide);
                                    break;
                                case 1:
                                    FighterReference().GetOpponent().ForcePlayerSide(FighterReference().Body.IsLeftSide);
                                    break;
                            }
                            break;
                        case SuperArmorAnimationEvent armorEvent: //Set super armor
                            Variables.SuperArmor = (sbyte)armorEvent.ArmorValue;
                            break;
                        case PlaySoundAnimationEvent soundEvent: //Set super armor
                            SoundEvents(soundEvent);
                            break;
                    }
                }
            }

            EventExecuted = true;
        }

        public void AnimationDamage(ApplyDamageAnimationEvent damageEvent)
        {
            SakugaFighter reference = null;
            
            switch (damageEvent.Index)
            {
                case 0:
                    reference = FighterReference();
                    break;
                case 1:
                    reference = FighterReference().GetOpponent();
                    break;
            }
            reference.HitStun.Start(Global.MinHitstun);

            int finalDamage = damageEvent.Value;
            if (damageEvent.AffectedByModifiers)
                finalDamage = reference.FighterVars.CalculateCompleteDamage(damageEvent.Value, reference.GetOpponent().FighterVars.CurrentAttack);

            switch (damageEvent.HealthChange)
            {
                case Global.ExtraVariableChange.SET:
                    reference.Variables.CurrentHealth = finalDamage;
                    break;
                case  Global.ExtraVariableChange.ADD:
                    reference.Variables.AddHealth(finalDamage);
                    break;
                case  Global.ExtraVariableChange.SUBTRACT:
                    reference.Variables.TakeDamage(finalDamage, 0, damageEvent.KillingBlow);
                    break;
            }
            if (damageEvent.AffectDamageTracker)
            {
                reference.Tracker.UpdateTrackers((uint)finalDamage, 0, Global.MinHitstun, 0, false);
                reference.GetOpponent().Tracker.FrameAdvantage = -(0 - Global.MinHitstun);
            }
        }

        public void SoundEvents(PlaySoundAnimationEvent soundEvent)
        {
            if (SFXList == null) return;
            if (VoicesList == null) return;
            
            int ind = soundEvent.IsRandom ? Global.RNG.Next(soundEvent.Index, soundEvent.Range) : soundEvent.Index;
            if (soundEvent.FromExtraVariable >= 0)
                ind = Variables.ExtraVariables[soundEvent.FromExtraVariable].CurrentValue;
            AudioStream selectedSound = null;
            switch ((int)soundEvent.SoundType)
            {
                case 0:
                    selectedSound = SFXList.Sounds[ind];
                    break;
                case 1:
                    selectedSound = VoicesList.Sounds[ind];
                    break;
            }
            Sounds[soundEvent.Source].QueueSound(selectedSound);
            if (soundEvent.FromExtraVariable >= 0)
                Variables.ExtraVariables[soundEvent.FromExtraVariable].ChangeOnUse();
        }

        public Vector2I Teleport(Vector2I Target, int index, Global.RelativeTo xRelative, Global.RelativeTo yRelative)
        {
            int relativePosX = 0;
            int relativePosY = 0;
            switch (xRelative)
            {
                case Global.RelativeTo.WORLD:
                    break;
                case Global.RelativeTo.SELF:
                    relativePosX = Body.FixedPosition.X;
                    Target.X *= Body.PlayerSide;
                    break;
                case Global.RelativeTo.OPPONENT:
                    relativePosX = FighterReference().GetOpponent().Body.FixedPosition.X;
                    Target.X *= FighterReference().GetOpponent().Body.PlayerSide;
                    break;
                case Global.RelativeTo.FIGHTER: //By index
                    switch (index)
                    {
                        case 0:
                            relativePosX = FighterReference().Body.FixedPosition.X;
                            Target.X *= FighterReference().Body.PlayerSide;
                            break;
                        case 1:
                            relativePosX = FighterReference().GetOpponent().Body.FixedPosition.X;
                            Target.X *= FighterReference().GetOpponent().Body.PlayerSide;
                            break;
                    }
                    break;
                case Global.RelativeTo.SPAWNABLE: //By index
                    relativePosX = FighterReference().GetActiveSpawnable(index).Body.FixedPosition.X;
                    Target.X *= FighterReference().GetActiveSpawnable(index).Body.PlayerSide;
                    break;
            }

            switch (yRelative)
            {
                case Global.RelativeTo.WORLD:
                    break;
                case Global.RelativeTo.SELF:
                    relativePosY = Body.FixedPosition.Y;
                    break;
                case Global.RelativeTo.OPPONENT:
                    relativePosY = FighterReference().GetOpponent().Body.FixedPosition.Y;
                    break;
                case Global.RelativeTo.FIGHTER: //By index
                    switch (index)
                    {
                        case 0:
                            relativePosY = FighterReference().Body.FixedPosition.Y;
                            break;
                        case 1:
                            relativePosY = FighterReference().GetOpponent().Body.FixedPosition.Y;
                            break;
                    }
                    break;
                case Global.RelativeTo.SPAWNABLE: //By index
                    relativePosY = FighterReference().GetActiveSpawnable(index).Body.FixedPosition.Y;
                    break;
            }

            Vector2I finalPosition = new Vector2I(Target.X + relativePosX, Target.Y + relativePosY);
            return finalPosition;
        }
	}
}
