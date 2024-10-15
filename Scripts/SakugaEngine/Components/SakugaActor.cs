using Godot;
using System.IO;
using SakugaEngine.Collision;
using System;
using SakugaEngine.Resources;

namespace SakugaEngine
{
	public partial class SakugaActor : Node3D
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
        //[Export] public SpawnsList SpawnablesList;
        [Export] public SpawnsList VFXList;
        [Export] public SoundsList SFXList;
        [Export] public SoundsList VoicesList;        
		
		public virtual void Tick(){}
		public virtual void LateTick(){}
		
		public virtual void Serialize(BinaryWriter bw){}
		public virtual void Deserialize(BinaryReader br){}

        protected virtual bool LifeEnded() { return false; }
        protected virtual SakugaFighter FighterReference() { return null; }
        public virtual bool AllowHitCheck(SakugaActor other) { return true; }

        public override void _Process(double delta)
        {
            Position = Global.ToScaledVector3(Body.FixedPosition);
            foreach (Node3D g in Graphics)
                g.Scale = new Vector3(Body.PlayerSide, 1, 1);
        }

		public void UpdateFighterPhysics()
        {
            if (Animator.GetCurrentState().statePhysics.Length == 0) return;

            for(int i = 0; i < Animator.GetCurrentState().statePhysics.Length; ++i)
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

		public void UpdateHitboxes(bool canHitAgain)
        {
            for (int i = 0; i < Animator.GetCurrentState().hitboxStates.Length; ++i)
            {
                if (Animator.Frame == Animator.GetCurrentState().hitboxStates[i].Frame)
                {
                    Body.SetHitbox(Animator.GetCurrentState().hitboxStates[i].HitboxIndex);
                    if (canHitAgain) Body.HitConfirmed = false;
                }
            }
        }

        public void UpdateFrameProperties()
        {
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
                    ((conditions & (byte)Global.TransitionCondition.ON_GROUND) != 0 && Body.IsOnGround);
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
                int distance = Global.HorizontalDistance(FighterReference().GetOpponent().Body.FixedPosition, Body.FixedPosition);
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

            for (int i = 0; i < Animator.GetCurrentState().animationEvents.Length; i++)
            {
                Vector2I dst = Teleport(Animator.GetCurrentState().animationEvents[i].TargetPosition,
                                        Animator.GetCurrentState().animationEvents[i].Index,
                                        (int)Animator.GetCurrentState().animationEvents[i].xRelativeTo,
                                        (int)Animator.GetCurrentState().animationEvents[i].yRelativeTo);
                                                        
                if (Animator.Frame != Animator.GetCurrentState().animationEvents[i].Frame) continue;

                switch ((int)Animator.GetCurrentState().animationEvents[i].Type)
                {
                    case 0: //Spawn Object (Spawnable, VFX)
                        int ind = Animator.GetCurrentState().animationEvents[i].IsRandom ? 
                            Global.RNGRange(Animator.GetCurrentState().animationEvents[i].Index, Animator.GetCurrentState().animationEvents[i].Range) : 
                            Animator.GetCurrentState().animationEvents[i].Index;
                        switch((int)Animator.GetCurrentState().animationEvents[i].Object)
                        {
                            case 0: //Spawnable
                                FighterReference().SpawnSpawnable(ind, dst);
                                break;
                            case 1: //VFX
                                FighterReference().SpawnVFX(ind, dst);
                                break;
                        }
                        break;
                    case 1: //Teleport
                        Body.MoveTo(dst);
                        break;
                    case 2: //Animation Damage
                        int damage = Animator.GetCurrentState().animationEvents[i].Value;
                        switch (Animator.GetCurrentState().animationEvents[i].Index)
                        {
                            case 0:
                                if (damage > 0) FighterReference().Variables.AddHealth(Mathf.Abs(damage));
                                else if (damage < 0) FighterReference().Variables.RemoveHealth(Mathf.Abs(damage));
                                break;
                            case 1:
                                if (damage > 0) FighterReference().GetOpponent().Variables.AddHealth(Mathf.Abs(damage));
                                else if (damage < 0) FighterReference().GetOpponent().Variables.RemoveHealth(Mathf.Abs(damage));
                                break;
                        }
                        break;
                    case 3: //Dettach Throw
                        FighterReference().DettachThrow();
                        break;
                    case 4: //Force Side Change
                        switch (Animator.GetCurrentState().animationEvents[i].Index)
                        {
                            case 0:
                                FighterReference().ForcePlayerSide();
                                break;
                            case 1:
                                FighterReference().GetOpponent().ForcePlayerSide();
                                break;
                        }
                        break;
                    case 5: //Set super armor
                        Variables.SuperArmor = (sbyte)Animator.GetCurrentState().animationEvents[i].Value;
                        GD.Print("Super Armor: " + Animator.GetCurrentState().animationEvents[i].Value);
                        break;
                }
            }
        }

        public void SoundEvents(SoundsList SFX, SoundsList VoiceLines)
        {
            if (Animator.GetCurrentState().soundEvents.Length <= 0) return;
            if (SFX == null) return;
            if (VoiceLines == null) return;

            for (int i = 0; i < Animator.GetCurrentState().soundEvents.Length; i++)
            {
                if (Animator.Frame != Animator.GetCurrentState().soundEvents[i].Frame) continue;
                int ind = Animator.GetCurrentState().soundEvents[i].IsRandom ? 
                    Global.RNGRange(Animator.GetCurrentState().soundEvents[i].Index, Animator.GetCurrentState().soundEvents[i].Range) : 
                    Animator.GetCurrentState().soundEvents[i].Index;
                AudioStream selectedSound = null;
                switch ((int)Animator.GetCurrentState().soundEvents[i].SoundType)
                {
                    case 0:
                        selectedSound = SFX.Sounds[ind];
                        break;
                    case 1:
                        selectedSound = VoiceLines.Sounds[ind];
                        break;
                }
                Sounds[Animator.GetCurrentState().soundEvents[i].Source].QueueSound(selectedSound);
            }
        }

        public Vector2I Teleport(Vector2I Target, int index, int xRelative, int yRelative)
        {
            int relativePosX = 0;
            int relativePosY = 0;
            switch (xRelative)
            {
                case 0: //World
                    break;
                case 1: //Self
                    relativePosX = Body.FixedPosition.X;
                    Target.X *= Body.PlayerSide;
                    break;
                case 2: //Fighter
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
                case 3: //Spawnable
                    relativePosX = FighterReference().GetActiveSpawnable(index).Body.FixedPosition.X;
                    Target.X *= FighterReference().GetActiveSpawnable(index).Body.PlayerSide;
                    break;
            }

            switch (yRelative)
            {
                case 0:
                    break;
                case 1:
                    relativePosY = Body.FixedPosition.Y;
                    break;
                case 2:
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
                case 3:
                    relativePosY = FighterReference().GetActiveSpawnable(index).Body.FixedPosition.Y;
                    break;
            }

            Vector2I finalPosition = new Vector2I(Target.X + relativePosX, Target.Y + relativePosY);
            return finalPosition;
        }
	}
}
