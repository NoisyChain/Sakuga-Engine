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

        //[ExportCategory("Variables")]
        
        //public int CurrentState;
        public bool SuperStop;
        private bool isBeingPushed;
        private FighterBody _opponent;

        public FighterBody GetOpponent() => _opponent;
        public void SetOpponent(FighterBody opponent) { _opponent = opponent; }

        public override void _Process(double delta)
        {
            Position = Global.ToScaledVector3(Body.FixedPosition);
        }

        public void ParseInputs(ushort rawInputs)
        {
            Inputs.InsertToHistory(rawInputs);
        }

        public void Initialize(int StartingPosition)
        {
            Body.Initialize(this);
            Body.FixedPosition.X = StartingPosition;
            CallState(Stance.GetCurrentStance().NeutralState);
            Animator.Frame = -1;
            Variables.Initialize();
            Stance.Initialize(this);
        }

        public void ChangeSide(bool leftSide)
        {
            if (Body.IsLeftSide == leftSide) return;
            if (!Body.IsOnGround) return;
            if (!Stance.CanAutoTurn()) return;

            Body.IsLeftSide = leftSide;

            if (Stance.currentMove >= 0 &&(int)Stance.GetCurrentMove().SideChange == 2)
                Stance.ResetStance();
        }

        public void Tick()
        {
            Inputs.InputSide = Body.IsLeftSide ? 1 : -1;

            if (!HitStop.IsRunning())
            {
                HitStun.Run();
                Animator.RunState();
                StateTransitions();
                Animator.LoopState();
            }
            HitStop.Run();
            MoveBuffer.Run();
            Stance.CheckMoves();
            UpdateHitboxes();
            UpdateFighterPhysics();
        }

        private void UpdateFighterPhysics()
        {
            if (isBeingPushed) return;

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

#region Return functions
        public bool IsKO() => Variables.CurrentHealth <= 0;
#endregion

#region Interface functions
        public void BaseDamage(HitboxElement box, Vector2I contact){}
        public void ThrowDamage(HitboxElement box){}
        public void ProjectileDamage(HitboxElement box, Vector2I contact, int priority){}
        public void HitboxClash(HitboxElement box, Vector2I contact, int priority){}
        public void ProjectileDeflect(HitboxElement box){}
        public void CounterHit(HitboxElement box, Vector2I contact){}
        public void ProximityBlock(){}
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
            //Variables
            bw.Write(SuperStop);
            bw.Write(isBeingPushed);
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
            //Variables
            SuperStop = br.ReadBoolean();
            isBeingPushed = br.ReadBoolean();

            Body.UpdateColliders();
        }
#endregion
    }
}
