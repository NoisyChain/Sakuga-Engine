using Godot;
using SakugaEngine.Resources;
using System;
using SakugaEngine.Collision;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class FighterBody : SakugaNode
    {
        [ExportCategory("Components")]
        [Export] public PhysicsBody Body;
        [Export] public InputManager Inputs;
        [Export] public FighterVariables Variables;
        [Export] public FrameAnimator Animator;
        [Export] public StanceManager Stance;
    
        [ExportCategory("Timers")]
        [Export] public FrameTimer HitStun;
        [Export] public FrameTimer HitStop;
        [Export] public FrameTimer  MoveBuffer;

        [ExportCategory("Variables")]
        [Export] public FighterState[] States;

        public int CurrentState;
        public bool SuperStop;
        private bool isBeingPushed;

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
            CurrentState = Stance.GetCurrentStance().NeutralState;
            Body.Initialize();
            Body.FixedPosition.X = StartingPosition;
            CallState(CurrentState);
            Animator.Frame = -1;
            Variables.Initialize();
            Stance.Initialize(this);
        }

        public void ChangeSide(bool leftSide)
        {
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
            UpdateFighterPhysics();
        }

        private void UpdateFighterPhysics()
        {
            if (isBeingPushed) return;

            if (GetCurrentState().statePhysics.Length == 0) return;

            for(int i = 0; i < GetCurrentState().statePhysics.Length; ++i)
            {
                int nextFrame = i + 1 < GetCurrentState().statePhysics.Length ? GetCurrentState().statePhysics[i + 1].frame : GetCurrentState().Duration;
                if (Animator.Frame >= GetCurrentState().statePhysics[i].frame && Animator.Frame < nextFrame)
                {
                    if (GetCurrentState().statePhysics[i].UseLateralSpeed)
                        Body.SetLateralVelocity(GetCurrentState().statePhysics[i].LateralSpeed * Inputs.InputSide);
                    if (GetCurrentState().statePhysics[i].UseVerticalSpeed)
                        Body.SetVerticalVelocity(GetCurrentState().statePhysics[i].VerticalSpeed);
                    if (GetCurrentState().statePhysics[i].UseGravity)
                        Body.AddGravity(GetCurrentState().statePhysics[i].Gravity);
                }
            }
        }

        public void CallState(int index, bool reset = false)
        {
            CurrentState = index;
            Animator.PlayState(GetCurrentState(), reset);
        }

        public void StateTransitions()
        {
            if (GetCurrentState().stateTransitions.Length <= 0) return;

            bool ValidTransition = false;

            for (int i = 0; i < GetCurrentState().stateTransitions.Length; i++)
            {
                if (GetCurrentState().stateTransitions[i].StateIndex < 0) continue;
                
                switch ((int)GetCurrentState().stateTransitions[i].Condition)
                {
                    case 0: //State End
                        ValidTransition = Animator.Frame >= GetCurrentState().Duration;
                        break;
                    case 1: //At Frame
                        ValidTransition = Animator.Frame >= GetCurrentState().stateTransitions[i].AtFrame;
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
                        ValidTransition = Inputs.CheckMotionInputs(GetCurrentState().stateTransitions[i].Inputs);
                        break;
                }

                if (ValidTransition) CallState(GetCurrentState().stateTransitions[i].StateIndex);
            }
        }

#region Return functions
        public FighterState GetCurrentState() => States[CurrentState];
        public int StateType() => (int)GetCurrentState().Type;
#endregion
    }
}
