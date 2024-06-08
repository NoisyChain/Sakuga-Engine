using Godot;
using SakugaEngine.Resources;
using System;
using System.Data;

namespace SakugaEngine
{
    public partial class FighterBody : PhysicsBody
    {
        [Export] public InputManager Inputs;
        [Export] public FighterVariables Variables;
        [Export] public FrameAnimator Animator;
        [Export] public FighterState[] States;
        [Export] public FighterStance[] Stances;
        public int CurrentState;
        public int CurrentStance;
        //[Export] public int RunSpeed;
        //[Export] public int JumpSpeed;
        //[Export] public int Gravity;
        public int HitStun;
        public int HitStop;
        public bool SuperStop;

        public void ParseInputs(ushort rawInputs)
        {
            //Inputs.InsertToHistory(rawInputs);
            Inputs.Parse(rawInputs);
        }

        public void Initialize(int StartingPosition)
        {
            PlayerSide = -Mathf.Sign(StartingPosition);
            CurrentStance = 0;
            CurrentState = GetCurrentStance().NeutralState;
            FixedPosition.X = StartingPosition;
            CallState(CurrentState);
            Animator.Frame = -1;
            Variables.Initialize();
            foreach (FighterStance stance in Stances)
                stance.Initialize(this);
        }

        public void Tick()
        {
            //Inputs.ChargeDirectionalInputs();
            if (GetCurrentStance().MoveBuffer > 0) GetCurrentStance().MoveBuffer--;
            Inputs.InputSide = PlayerSide;
            Animator.RunState();
            StateTransitions();
            Animator.LoopState();
            GetCurrentStance().CheckMoves();
            /*if (IsOnGround)
            {
                if (Inputs.v.isPositive())
                    SetVerticalVelocity(JumpSpeed);
                else
                    SetVerticalVelocity(0);
                
                if (Inputs.h.isPositive())
                    SetLateralVelocity(RunSpeed);
                else if (Inputs.h.isNegative())
                    SetLateralVelocity(-RunSpeed);
                else
                    SetLateralVelocity(0);
            }
            else
            {
                AddGravity(Gravity);
            }*/
            UpdateFighterPhysics();
        }

        private void UpdateFighterPhysics()
        {
            //if (isBeingPushed) return;
            if (GetCurrentState().statePhysics.Length == 0) return;

            for(int i = 0; i < GetCurrentState().statePhysics.Length; ++i)
            {
                int nextFrame = i + 1 < GetCurrentState().statePhysics.Length ? GetCurrentState().statePhysics[i + 1].frame : GetCurrentState().Duration;
                if (Animator.Frame >= GetCurrentState().statePhysics[i].frame && Animator.Frame < nextFrame)
                {
                    if (GetCurrentState().statePhysics[i].UseLateralSpeed)
                        SetLateralVelocity(GetCurrentState().statePhysics[i].LateralSpeed * PlayerSide);
                    if (GetCurrentState().statePhysics[i].UseVerticalSpeed)
                        SetVerticalVelocity(GetCurrentState().statePhysics[i].VerticalSpeed);
                    if (GetCurrentState().statePhysics[i].UseGravity)
                        AddGravity(GetCurrentState().statePhysics[i].Gravity);
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
                        ValidTransition = IsOnGround;
                        break;
                    case 3: //On Walls
                        ValidTransition = IsOnWall;
                        break;
                    case 4: //On Fall
                        ValidTransition = FixedVelocity.Y < 0;
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
        public FighterStance GetCurrentStance() => Stances[CurrentStance];
#endregion
    }
}
