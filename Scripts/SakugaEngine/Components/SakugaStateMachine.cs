using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class SakugaStateMachine : Node
    {
        private SakugaFighter _owner;

        public bool CanRun;

        public int CurrentStance;
        public int BufferedMove = -1;
        public int CurrentMove = -1;
        public bool CancelBuffer;

        public void Initialize(SakugaFighter owner)
        {
            _owner = owner;
            CurrentStance = 0;
        }

        public bool CheckMoveConditions(int index)
        {
            if (_owner.Body.ContainsFrameProperty((byte)Global.FrameProperties.LOCK_MOVE)) return false;

            if (CurrentMove >= 0)
            {
                if (CurrentMove == index)
                {
                    bool canOverride = !GetCurrentMove().CanBeOverrided || (GetCurrentMove().CanBeOverrided && GetCurrentMove().CanOverrideToSelf);

                    if (!canOverride) return false;
                }

                if (GetMove(index).PriorityBuffer && GetMove(index).Priority < GetCurrentMove().Priority) return false;
            }

            int distance = Global.Distance(_owner.GetOpponent().Body.FixedPosition, _owner.Body.FixedPosition).X;
            bool isValidDistance = distance >= GetMove(index).DistanceArea.X && distance <= GetMove(index).DistanceArea.Y;
            if (!isValidDistance) return false;

            bool isCorrectSurface = (GetMove(index).UseOnGround && _owner.Body.IsOnGround) ||
                (GetMove(index).UseOnAir && !_owner.Body.IsOnGround && _owner.Body.FixedPosition.Y >= GetMove(index).MinimumHeight) ||
                (GetMove(index).UseOnGround && GetMove(index).UseOnAir);
            if (!isCorrectSurface) return false;

            bool isValidState = CheckStatesToIgnore(index);
            if (!isValidState) return false;

            bool isCorrectState = CheckOwnerState(index);
            if (!isCorrectState) return false;

            bool isDesiredHealth = _owner.Variables.CurrentHealth >= GetMove(index).HealthThreshold.X &&
                _owner.Variables.CurrentHealth <= GetMove(index).HealthThreshold.Y;
            if (!isDesiredHealth) return false;
            
            if (_owner.Variables.CurrentSuperGauge < GetMove(index).SuperGaugeRequired) return false;

            if (!_owner.Variables.CompareExtraVariables(GetMove(index).ExtraVariablesRequirement)) return false;
            
            bool AllowedFrameWindow = _owner.Animator.GetCurrentState().Loop || !_owner.Animator.GetCurrentState().Loop && _owner.Animator.CurrentStateFrame < GetMove(index).FrameLimit;
            if (GetMove(index).FrameLimit > 0 && !AllowedFrameWindow) return false;
            
            return true;
        }

        public void CheckMoves()
        {
            if (!CanRun) return;

            for (int i = GetMoveListLength() - 1; i >= 0; i--)
            {
                if (GetMove(i).SkipCheck) continue;
                if (!CheckMoveConditions(i)) continue;
                if (_owner.Inputs.CheckMotionInputs(GetMove(i).Inputs))
                {
                    BufferMove(i, false);
                    break;
                }
            }

            if (CurrentMove >= 0)
                _owner.Variables.AddSuperGauge(GetCurrentMove().BuildSuperGauge);

            CheckMoveEndCondition();
            CheckMoveCancel();
            ProcessMoveBuffer();
        }

        private void BufferMove(int moveIndex, bool moveCancel)
        {
            BufferedMove = moveIndex;
            CancelBuffer = moveCancel;
            _owner.MoveBuffer.Start(Global.MoveBufferLength);
        }

        public void ExecuteMove(int moveIndex)
        {
            if (GetMove(moveIndex).InterruptCornerPushback) _owner.StopPushing();
            _owner.Variables.CurrentSuperGauge -= GetMove(moveIndex).SuperGaugeRequired;

            if (_owner.Variables.CurrentHealth > 10 && GetMove(moveIndex).SpendHealth > 0)
                _owner.Variables.CurrentHealth -= GetMove(moveIndex).SpendHealth;

            if (GetMove(moveIndex).SuperFlash > 0 && !_owner.SuperFlash)
            {
                _owner.GetOpponent().SuperFlash = true;
                _owner.GetOpponent().HitStop.Start((uint)GetMove(moveIndex).SuperFlash);
            }

            if (GetMove(moveIndex).MoveState >= 0) 
                _owner.Animator.PlayState(GetMove(moveIndex).MoveState, true);
            
            _owner.Variables.ExtraVariablesOnMoveEnter();
            
            _owner.Variables.SetExtraVariables(GetMove(moveIndex).ExtraVariablesChange);

            CurrentMove = moveIndex;
            BufferedMove = -1;
            CancelBuffer = false;

            GD.Print("You executed " + GetCurrentMove().MoveName + "!");
        }

        private void CheckMoveCancel()
        {
            if (CurrentMove < 0) return;
            if (GetCurrentMove().CanCancelTo == null) return;
            if (GetCurrentMove().CanCancelTo.Length <= 0) return;

            foreach (MoveCancelSettings cancel in GetCurrentMove().CanCancelTo)
            {
                if (!CheckCancelConditions(cancel)) continue;
                if (!CheckMoveConditions(cancel.MoveIndex)) continue;
                if (_owner.Inputs.CheckMotionInputs(GetMove(cancel.MoveIndex).Inputs))
                {
                    BufferMove(cancel.MoveIndex, true);
                    break;
                }
            }
        }

        public void ProcessMoveBuffer()
        {
            if (BufferedMove < 0) return;
            if (!CancelBuffer && _owner.Animator.CurrentStateType() == Global.StateType.COMBAT) return;
            if (!GetBufferedMove().IgnoreHitstop && _owner.HitStop.IsRunning()) return;
            if (!GetBufferedMove().IgnoreHitstun && _owner.HitStun.IsRunning()) return;

            if (!_owner.MoveBuffer.IsRunning())
            {
                BufferedMove = -1;
                GD.Print("Buffer Cleaned!");
            }
            else
            {
                bool CanOverride = CurrentMove < 0 || CancelBuffer || (GetCurrentMove().CanBeOverrided && 
                                    (GetCurrentMove().IgnoreSamePriority ? 
                                    GetBufferedMove().Priority > GetCurrentMove().Priority : 
                                    GetBufferedMove().Priority >= GetCurrentMove().Priority));

                bool checkMovementState = GetBufferedMove().AcceptMovementStates && _owner.Animator.GetCurrentState().Type == Global.StateType.MOVEMENT;
                bool checkNullState = GetBufferedMove().AcceptNullStates && _owner.Animator.GetCurrentState().Type == Global.StateType.NULL;
                bool checkCombatState = GetBufferedMove().AcceptCombatStates && _owner.Animator.GetCurrentState().Type == Global.StateType.COMBAT;
                bool checkBlockingState = GetBufferedMove().AcceptBlockingStates && _owner.Animator.GetCurrentState().Type == Global.StateType.BLOCKING;
                bool checkHitReactionState = GetBufferedMove().AcceptHitReactionStates && _owner.Animator.GetCurrentState().Type == Global.StateType.HIT_REACTION;
                                    
                bool isValidStateType =  _owner.Animator.GetCurrentState().Type != Global.StateType.LOCKED && 
                                        (checkMovementState || checkNullState || checkCombatState || checkBlockingState || checkHitReactionState);
                
                if (isValidStateType && CanOverride)
                {
                    ExecuteMove(BufferedMove);
                }
                else GD.Print("Move " + GetBufferedMove().MoveName + " Buffered!");
            }
        }

        private void CheckMoveEndCondition()
        {
            if (CurrentMove < 0) return;

            switch (GetCurrentMove().MoveEnd)
            {
                case Global.MoveEndCondition.STATE_END:
                    if (_owner.Animator.CurrentState != GetCurrentMove().MoveState)
                        ResetStance();
                    break;
                case Global.MoveEndCondition.RELEASE_BUTTON:
                    if (_owner.Inputs.CheckInputEnd(GetCurrentMove().Inputs))
                        ResetStance();
                    break;
                case Global.MoveEndCondition.STATE_TYPE_CHANGE:
                    if (_owner.Animator.CurrentStateType() != _owner.Data.States[GetCurrentMove().MoveState].Type)
                        ResetStance();
                    break;
                case Global.MoveEndCondition.ON_FALL:
                    if (_owner.Body.IsFalling)
                        ResetStance();
                    break;
            }
        }

        public void ResetStance()
        {
            if (GetCurrentMove().MoveEndState >= 0)
                _owner.Animator.PlayState(GetCurrentMove().MoveEndState, false);
                        
            if (GetCurrentMove().ChangeStance >= 0)
            {
                CurrentStance = GetCurrentMove().ChangeStance;
                BufferedMove = -1;
                CancelBuffer = false;
            }
            CurrentMove = -1;
            _owner.Variables.ExtraVariablesOnMoveExit();
        }

        public void Clear()
        {
            CurrentMove = -1;
            if (!GetCurrentStance().IsDamagePersistent) CurrentStance = 0;
        }

        public bool CheckOwnerState(int index)
        {
            if (GetMove(index).IsSequenceFromStates == null) return true;
            if (GetMove(index).IsSequenceFromStates.Length <= 0) return true;
            
            foreach (int possibleStates in GetMove(index).IsSequenceFromStates)
            {
                if (_owner.Animator.CurrentState == possibleStates)
                    return true;
            }
            return false;
        }

        public bool CheckStatesToIgnore(int index)
        {
            if (GetMove(index).IgnoreStates == null) return true;
            if (GetMove(index).IgnoreStates.Length <= 0) return true;
            
            foreach (int possibleStates in GetMove(index).IgnoreStates)
            {
                if (_owner.Animator.CurrentState == possibleStates)
                    return false;
            }
            return true;
        }

        private bool CheckCancelConditions(MoveCancelSettings cancelSettings)
        {
            bool isKaraCancel = ((_owner.CancelConditions & cancelSettings.Conditions) & Global.CancelCondition.KARA_CANCEL) > 0;
            if ((_owner.CancelConditions & cancelSettings.Conditions) > 0 && ((isKaraCancel && _owner.Animator.CurrentStateFrame < Global.KaraCancelWindow) ||
            (!isKaraCancel && _owner.Animator.CurrentStateFrame >= cancelSettings.FrameThreshold.X && _owner.Animator.CurrentStateFrame <= cancelSettings.FrameThreshold.Y)))
            {
                return true;
            }
            return false;
        }

        

        public FighterStance GetCurrentStance() => _owner.Data.Stances[CurrentStance];
        public MoveSettings GetMove(int index) => GetCurrentStance().Moves[index];
        public MoveSettings GetCurrentMove() => GetMove(CurrentMove);
        public MoveSettings GetBufferedMove() => GetMove(BufferedMove);
        public int GetMoveListLength() => GetCurrentStance().Moves.Length;
        public bool CanAutoTurn() => CurrentMove < 0 || GetCurrentMove().SideChange > 0;

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(CurrentStance);
            bw.Write(BufferedMove);
            bw.Write(CurrentMove);
            bw.Write(CancelBuffer);
        }

        public void Deserialize(BinaryReader br)
        {
            CurrentStance = br.ReadInt32();
            BufferedMove = br.ReadInt32();
            CurrentMove = br.ReadInt32();
            CancelBuffer = br.ReadBoolean();
        }
    }
}