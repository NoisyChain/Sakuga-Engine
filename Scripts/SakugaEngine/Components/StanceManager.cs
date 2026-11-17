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
            if (!GlobalFunctions.Assert(index >= 0, "Move index is too low. Only positive move indices are supported.")) return false;
            if (!GlobalFunctions.Assert(index < GetMoveListLength(), "Move index is too high. Please call an existing move or add a new move to your character.")) return false;
            
            var nextMove = GetMove(index);
            if (!GlobalFunctions.Assert(nextMove != null, $"Null/invalid move found at index {index}.")) return false;

            if (!CheckAllowedStateType(index)) return false;
            
            if (CurrentMove >= 0)
            {
                if (CurrentMove == index && nextMove.IgnoreSelf) return false;
                if (nextMove.PriorityBuffer && nextMove.Priority < GetCurrentMove().Priority) return false;
            }
            if (nextMove.AcceptHitReactionStates && nextMove.IgnoreKnockdownHits && _owner.HitstunType >= HitstunType.KNOCKDOWN) return false;

            if (_owner.Parameters.SuperGauge.CurrentValue < nextMove.SuperGaugeRequired) return false;
            if (!_owner.Parameters.BurstGauge.Locked && _owner.Parameters.BurstGauge.CurrentValue < nextMove.BurstRequired) return false;

            bool isDesiredHealth = _owner.Parameters.Health.CurrentValue >= nextMove.HealthRequired.X &&
                _owner.Parameters.Health.CurrentValue <= nextMove.HealthRequired.Y;
            if (!isDesiredHealth) return false;

            int distance = GlobalFunctions.Distance(_owner.GetOpponent(0).Body.FixedPosition, _owner.Body.FixedPosition).X;
            bool isValidDistance = distance >= nextMove.DistanceArea.X && distance <= nextMove.DistanceArea.Y;
            if (!isValidDistance) return false;

            if (!IsCorrectSurface(index)) return false;

            bool isValidState = CheckStatesToIgnore(index);
            if (!isValidState) return false;

            bool isCorrectState = CheckOwnerState(index);
            if (!isCorrectState) return false;

            if (!_owner.Parameters.CompareVariables(nextMove.VariablesRequirement)) return false;
            
            bool AllowedFrameWindow = _owner.StateManager.CurrentStateFrame < nextMove.FrameLimit;
            if (nextMove.FrameLimit > 0 && !AllowedFrameWindow) return false;
            
            return true;
        }

        public void CheckMoves()
        {
            if (MoveBuffer == null) return;

            for (int i = GetMoveListLength() - 1; i >= 0; i--)
            {
                if (!CanRun) break;
                if (GetMove(i).SkipCheck) continue;
                if (!CheckMoveConditions(i)) continue;
                if (_owner.Inputs.CheckMotionInputs(GetMove(i).Inputs, _owner.InputSide(GetMove(i).SideCheck)))
                {
                    BufferMove(i, false);
                    break;
                }
            }

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
            MoveSettings moveToExecute = GetMove(moveIndex);

            _owner.Parameters.SuperGauge.SpendSuperGauge(moveToExecute.SuperGaugeRequired);
            if (moveToExecute.SpendBurst) _owner.Parameters.BurstGauge.SpendBurstGauge(moveToExecute.BurstRequired);

            if (moveToExecute.MoveState >= 0) 
                _owner.StateManager.PlayState(moveToExecute.MoveState, true);
            
            _owner.Parameters.ChangeVariablesBehavior(CustomVariableBehaviorTarget.ON_MOVE_ENTER);
            _owner.Parameters.SetVariables(moveToExecute.VariablesChange);

            CurrentMove = moveIndex;
            BufferedMove = -1;
            CancelBuffer = false;
            MoveBuffer.Stop();

            GD.Print("You executed " + GetCurrentMove().MoveName + "!");
        }

        private void CheckMoveCancel()
        {
            if (CurrentMove < 0) return;
            MoveSettings currentMove = GetCurrentMove();
            if (currentMove.CanCancelTo == null) return;
            if (currentMove.CanCancelTo.Length <= 0) return;

            foreach (MoveCancelSettings cancel in currentMove.CanCancelTo)
            {
                if (!CheckCancelConditions(cancel)) continue;
                if (!CheckMoveConditions(cancel.MoveIndex)) continue;
                if (_owner.Inputs.CheckMotionInputs(GetMove(cancel.MoveIndex).Inputs, _owner.InputSide(GetMove(cancel.MoveIndex).SideCheck)))
                {
                    BufferMove(cancel.MoveIndex, true);
                    break;
                }
            }
        }

        public void ProcessMoveBuffer()
        {
            if (BufferedMove < 0) return;
            MoveSettings bufferedMove = GetBufferedMove();
            
            if (!CancelBuffer && _owner.StateManager.CurrentStateType() == StateType.COMBAT) return;
            if (!CheckAllowedStateType(BufferedMove)) MoveBuffer.Stop();
            if (!MoveBuffer.IsRunning()) { BufferedMove = -1; GD.Print("Buffer Cleaned!"); return; }
            if (!IsCorrectSurface(BufferedMove)) { BufferedMove = bufferedMove.MoveOverride; GD.Print("Buffer Overrided!"); return; }

            if (!bufferedMove.Unlockable && _owner.ContainsFrameProperty(FrameProperties.LOCK_MOVE)) return;
            if (!bufferedMove.IgnoreHitstop && _owner.Hitstop.IsRunning()) return;
            if (!bufferedMove.IgnoreHitstun && _owner.Hitstun.IsRunning()) return;
            
            bool CanOverride = CurrentMove < 0 || CancelBuffer || (GetCurrentMove().CanBeOverrided && (GetCurrentMove().IgnoreSamePriority ? 
                                                                    bufferedMove.Priority > GetCurrentMove().Priority :
                                                                    bufferedMove.Priority >= GetCurrentMove().Priority));

            if (CanOverride) ExecuteMove(BufferedMove);
            else GD.Print("Move " + bufferedMove.MoveName + " Buffered!");
        }

        public bool IsCorrectSurface(int moveIndex)
        {
            MoveSettings nextMove = GetMove(moveIndex);
            return (nextMove.UseOnGround && _owner.Body.IsOnGround) ||
                (nextMove.UseOnAir && !_owner.Body.IsOnGround && _owner.Body.FixedPosition.Y >= nextMove.MinimumHeight) ||
                (nextMove.UseOnGround && nextMove.UseOnAir);
        }

        public bool CheckAllowedStateType(int moveIndex)
        {
            MoveSettings nextMove = GetMove(moveIndex);
            StateType currentStateType = _owner.StateManager.GetCurrentState().Type;
            bool notLockedState = currentStateType != StateType.LOCKED;
            bool checkNullState = nextMove.AcceptNullStates && currentStateType == StateType.NULL;
            bool checkMovementState = nextMove.AcceptMovementStates && currentStateType == StateType.MOVEMENT;
            bool checkCombatState = nextMove.AcceptCombatStates && currentStateType == StateType.COMBAT;
            bool checkBlockingState = nextMove.AcceptBlockingStates && currentStateType == StateType.BLOCKING;
            bool checkHitReactionState = nextMove.AcceptHitReactionStates && currentStateType == StateType.HIT_REACTION && _owner.HitstunType < HitstunType.HARD_KNOCKDOWN;

            return notLockedState && (checkNullState || checkMovementState || checkCombatState || checkBlockingState || checkHitReactionState);
        }

        private void CheckMoveEndCondition()
        {
            if (CurrentMove < 0) return;

            MoveSettings currentMove = GetCurrentMove();

            switch (currentMove.MoveEnd)
            {
                case MoveEndCondition.STATE_END:
                    if (_owner.StateManager.CurrentState != currentMove.MoveState || _owner.StateManager.StateEnded())
                        EndMove();
                    break;
                case MoveEndCondition.RELEASE_BUTTON:
                    if (_owner.Inputs.CheckInputEnd(currentMove.Inputs, _owner.InputSide(currentMove.SideCheck)))
                        EndMove();
                    break;
                case MoveEndCondition.STATE_TYPE_CHANGE:
                    if (_owner.StateManager.CurrentStateType() != _owner.Data.States[currentMove.MoveState].Type)
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
            MoveSettings currentMove = GetCurrentMove();
            if (currentMove.MoveEndState >= 0)
                _owner.StateManager.PlayState(currentMove.MoveEndState, false);

            CurrentMove = -1;
            _owner.Parameters.ChangeVariablesBehavior(CustomVariableBehaviorTarget.ON_MOVE_EXIT);
        }

        public void CallStance(int nextStance)
        {
            if (!GlobalFunctions.Assert(nextStance >= 0, "Stance index is too low. Only positive stance indices are supported.")) return;
            if (!GlobalFunctions.Assert(nextStance < _owner.Data.Stances.Length, "Stance index is too high. Please call an existing stance or add a new stance to your character.")) return;
            if (!GlobalFunctions.Assert(GetStance(nextStance) != null, $"Null/invalid stance found at index {nextStance}.")) return;
            
            CurrentStance = nextStance;
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

        public int GetBlockState(HitType Type, byte state)
        {
            if (_owner.StateManager.CurrentStateType() == StateType.HIT_REACTION) return -1;
            if (_owner.StateManager.CurrentStateType() == StateType.LOCKED) return -1;
            if (_owner.StateManager.CurrentStateType() == StateType.COMBAT && _owner.StateManager.CurrentStateFrame >= GlobalVariables.KaraCancelWindow) return -1;
            if (_owner.ContainsFrameProperty(FrameProperties.BLOCK_DISABLED)) return -1;
            if (_owner.ContainsFrameProperty(FrameProperties.LOCK_MOVE)) return -1;
            if (GetCurrentStance().BlockReactions == null || GetCurrentStance().BlockReactions.Length == 0) return -1;
            for (int i = 0; i < GetCurrentStance().BlockReactions.Length; i++)
            {
                BlockSettings block = GetCurrentStance().BlockReactions[i];
                if (_owner.Body.IsOnGround && block.ReferenceStance == StanceSelect.AIR) continue;
                if (!_owner.Body.IsOnGround && block.ReferenceStance != StanceSelect.AIR) continue;
                if (!ValidHitType(Type, block.BlockType)) continue;
                if (block.InputToCheck == null) continue;
                if (!_owner.Inputs.CheckMotionInputs(block.InputToCheck, _owner.InputSide(InputSideCheck.SIDE_RELATIVE))) continue;

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