using Godot;
using SakugaEngine.Resources;
using SakugaEngine.Global;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class StanceManager : Node
    {
        private SakugaActor _owner;
        [Export] public FrameTimer MoveBuffer;

        public bool CanRun;

        public int CurrentStance;
        public int BufferedMove = -1;
        public int CurrentMove = -1;
        public bool CancelBuffer;

        public void Initialize(SakugaActor owner)
        {
            _owner = owner;
            CurrentStance = 0;
        }

        public bool CheckMoveConditions(int index)
        {
            if (_owner.ContainsFrameProperty(Global.FrameProperties.LOCK_MOVE)) return false;

            if (!CheckAllowedStateType(index)) return false;

            if (CurrentMove >= 0)
            {
                if (CurrentMove == index)
                {
                    bool canOverride = !GetCurrentMove().CanBeOverrided || (GetCurrentMove().CanBeOverrided && GetCurrentMove().CanOverrideToSelf);

                    if (!canOverride) return false;
                }

                if (GetMove(index).PriorityBuffer && GetMove(index).Priority < GetCurrentMove().Priority) return false;
            }

            int distance = GlobalFunctions.Distance(_owner.GetOpponent(0).Body.FixedPosition, _owner.Body.FixedPosition).X;
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

            bool isDesiredHealth = _owner.Parameters.Health.CurrentValue >= GetMove(index).HealthRequired.X &&
                _owner.Parameters.Health.CurrentValue <= GetMove(index).HealthRequired.Y;
            if (!isDesiredHealth) return false;
            
            if (_owner.Parameters.SuperGauge.CurrentValue < GetMove(index).SuperGaugeRequired) return false;

            if (!_owner.Parameters.CompareVariables(GetMove(index).VariablesRequirement)) return false;
            
            bool AllowedFrameWindow = _owner.StateManager.GetCurrentState().Loop || !_owner.StateManager.GetCurrentState().Loop && _owner.StateManager.CurrentStateFrame < GetMove(index).FrameLimit;
            if (GetMove(index).FrameLimit > 0 && !AllowedFrameWindow) return false;
            
            return true;
        }

        public void CheckMoves()
        {
            if (MoveBuffer == null) return;

            for (int i = GetMoveListLength() - 1; i >= 0; i--)
            {
                if (GetMove(i).SkipCheck) continue;
                if (!CheckMoveConditions(i)) continue;
                if (_owner.Inputs.CheckMotionInputs(GetMove(i).Inputs, _owner.InputSide))
                {
                    if (!CanRun) continue;
                    BufferMove(i, false);
                    break;
                }
            }

            //if (CurrentMove >= 0)
                //_owner.Parameters.SuperGauge.AddSuperGauge(GetCurrentMove().BuildSuperGauge);

            CheckMoveEndCondition();
            CheckMoveCancel();
            ProcessMoveBuffer();
        }

        private void BufferMove(int moveIndex, bool moveCancel)
        {
            BufferedMove = moveIndex;
            CancelBuffer = moveCancel;
            MoveBuffer.Start(GlobalVariables.MoveBufferLength);
        }

        public void ExecuteMove(int moveIndex)
        {
            _owner.Parameters.SuperGauge.CurrentValue -= GetMove(moveIndex).SuperGaugeRequired;

            if (GetMove(moveIndex).SuperFlash > 0 && !_owner.SuperFlashing)
            {
                _owner.GetOpponent(0).SuperFlashing = true;
                _owner.GetOpponent(0).Hitstop.Start((uint)GetMove(moveIndex).SuperFlash);
            }

            if (GetMove(moveIndex).MoveState >= 0) 
                _owner.StateManager.PlayState(GetMove(moveIndex).MoveState, true);
            
            _owner.Parameters.ChangeVariablesBehavior(CustomVariableBehaviorTarget.ON_MOVE_ENTER);
            _owner.Parameters.SetVariables(GetMove(moveIndex).VariablesChange);

            CurrentMove = moveIndex;
            BufferedMove = -1;
            CancelBuffer = false;
            MoveBuffer.Stop();

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
                if (_owner.Inputs.CheckMotionInputs(GetMove(cancel.MoveIndex).Inputs, _owner.InputSide))
                {
                    if (!CanRun) continue;
                    
                    BufferMove(cancel.MoveIndex, true);
                    break;
                }
            }
        }

        public void ProcessMoveBuffer()
        {
            if (BufferedMove < 0) return;
            if (!MoveBuffer.IsRunning())
            {
                BufferedMove = -1;
                GD.Print("Buffer Cleaned!");
                return;
            }
            if (!CancelBuffer && _owner.StateManager.CurrentStateType() == StateType.COMBAT) return;
            if (!GetBufferedMove().IgnoreHitstop && _owner.Hitstop.IsRunning()) return;
            if (!GetBufferedMove().IgnoreHitstun && _owner.Hitstun.IsRunning()) return;

            
            bool CanOverride = CurrentMove < 0 || CancelBuffer || (GetCurrentMove().CanBeOverrided && 
                                (GetCurrentMove().IgnoreSamePriority ? 
                                GetBufferedMove().Priority > GetCurrentMove().Priority : 
                                GetBufferedMove().Priority >= GetCurrentMove().Priority));

            if (CanOverride)
            {
                ExecuteMove(BufferedMove);
            }
            else GD.Print("Move " + GetBufferedMove().MoveName + " Buffered!");
        }

        public bool CheckAllowedStateType(int moveIndex)
        {
            bool notLockedState =  _owner.StateManager.GetCurrentState().Type != StateType.LOCKED;
            bool checkNullState = GetMove(moveIndex).AcceptNullStates && _owner.StateManager.GetCurrentState().Type == StateType.NULL;
            bool checkMovementState = GetMove(moveIndex).AcceptMovementStates && _owner.StateManager.GetCurrentState().Type == StateType.MOVEMENT;
            bool checkCombatState = GetMove(moveIndex).AcceptCombatStates && _owner.StateManager.GetCurrentState().Type == StateType.COMBAT;
            bool checkBlockingState = GetMove(moveIndex).AcceptBlockingStates && _owner.StateManager.GetCurrentState().Type == StateType.BLOCKING;
            bool checkHitReactionState = GetMove(moveIndex).AcceptHitReactionStates && _owner.StateManager.GetCurrentState().Type == StateType.HIT_REACTION;

            return notLockedState && (checkNullState || checkMovementState || checkCombatState || checkBlockingState || checkHitReactionState);
        }

        private void CheckMoveEndCondition()
        {
            if (CurrentMove < 0) return;

            switch (GetCurrentMove().MoveEnd)
            {
                case MoveEndCondition.STATE_END:
                    if (_owner.StateManager.CurrentState != GetCurrentMove().MoveState)
                        EndMove();
                    break;
                case MoveEndCondition.RELEASE_BUTTON:
                    if (_owner.Inputs.CheckInputEnd(GetCurrentMove().Inputs, _owner.InputSide))
                        EndMove();
                    break;
                case MoveEndCondition.STATE_TYPE_CHANGE:
                    if (_owner.StateManager.CurrentStateType() != _owner.Data.States[GetCurrentMove().MoveState].Type)
                        EndMove();
                    break;
                case MoveEndCondition.ON_FALL:
                    if (_owner.Body.IsFalling)
                        EndMove();
                    break;
            }
        }

        public void EndMove()
        {
            if (GetCurrentMove().MoveEndState >= 0)
                _owner.StateManager.PlayState(GetCurrentMove().MoveEndState, false);

            CurrentMove = -1;
            _owner.Parameters.ChangeVariablesBehavior(CustomVariableBehaviorTarget.ON_MOVE_EXIT);
        }

        public void CallStance(int NextStance)
        {
            CurrentStance = NextStance;
            CurrentMove = -1;
            BufferedMove = -1;
            CancelBuffer = false;
            MoveBuffer.Stop();
        }

        public void Clear()
        {
            CurrentMove = -1;
            BufferedMove = -1;
            CancelBuffer = false;
            if (!GetCurrentStance().IsDamagePersistent) CurrentStance = 0;
            MoveBuffer.Stop();
        }

        public bool CheckOwnerState(int index)
        {
            if (GetMove(index).IsSequenceFromStates == null) return true;
            if (GetMove(index).IsSequenceFromStates.Length <= 0) return true;
            
            foreach (int possibleStates in GetMove(index).IsSequenceFromStates)
            {
                if (_owner.StateManager.CurrentState == possibleStates)
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
                if (_owner.StateManager.CurrentState == possibleStates)
                    return false;
            }
            return true;
        }

        private bool CheckCancelConditions(MoveCancelSettings cancelSettings)
        {
            bool isKaraCancel = ((_owner.CancelConditions & cancelSettings.Conditions) & CancelCondition.KARA_CANCEL) > 0;
            if ((_owner.CancelConditions & cancelSettings.Conditions) > 0 && ((isKaraCancel && _owner.StateManager.CurrentStateFrame < GlobalVariables.KaraCancelWindow) ||
            (!isKaraCancel && _owner.StateManager.CurrentStateFrame >= cancelSettings.FrameThreshold.X && _owner.StateManager.CurrentStateFrame <= cancelSettings.FrameThreshold.Y)))
            {
                return true;
            }
            return false;
        }

        public int GetBlockState(StanceSelect stance, HitType Type, byte state)
        {
            if (GetCurrentStance().BlockReactions == null || GetCurrentStance().BlockReactions.Length == 0) return -1;
            for (int i = 0; i < GetCurrentStance().BlockReactions.Length; i++)
            {
                BlockSettings block = GetCurrentStance().BlockReactions[i];
                if (!IsValidStance(block.ReferenceStance, stance)) continue;
                if (!ValidHitType(Type, block.BlockType)) continue;
                if (block.InputToCheck == null) continue;
                if (!_owner.Inputs.CheckMotionInputs(block.InputToCheck, _owner.InputSide)) continue;

                switch (state)
                {
                    case 0:
                        return block.EnterState;
                    case 1:
                        return block.StunState;
                    case 2:
                        return block.GuardCrushState;
                }
            }
            return -1;
        }

        public int ExitBlockState(StanceSelect stance)
        {
            if (GetCurrentStance().BlockReactions == null || GetCurrentStance().BlockReactions.Length == 0) return -1;
            for (int i = 0; i < GetCurrentStance().BlockReactions.Length; i++)
            {
                BlockSettings block = GetCurrentStance().BlockReactions[i];
                if (!IsValidStance(block.ReferenceStance, stance)) continue;

                return block.ExitState;
            }
            return -1;
        }

        private bool IsValidStance(StanceSelect stance, StanceSelect compare)
        {
            return stance == compare;
        }

        private bool ValidHitType(HitType hitType, BlockType blockType)
        {
            if (hitType == HitType.UNBLOCKABLE) return false;
            if (hitType == HitType.HIGH && (blockType & BlockType.HIGH) > 0) return true;
            if (hitType == HitType.MEDIUM && (blockType & BlockType.MEDIUM) > 0) return true;
            if (hitType == HitType.LOW && (blockType & BlockType.LOW) > 0) return true;

            return false;
        }

        public FighterStance GetStance(int index) => _owner.Data.Stances[index];
        public FighterStance GetCurrentStance() => GetStance(CurrentStance);
        public MoveSettings GetMove(int index) => GetCurrentStance().Moves[index];
        public MoveSettings GetCurrentMove() => GetMove(CurrentMove);
        public MoveSettings GetBufferedMove() => GetMove(BufferedMove);
        public int GetMoveListLength() => GetCurrentStance().Moves.Length;
    }
}