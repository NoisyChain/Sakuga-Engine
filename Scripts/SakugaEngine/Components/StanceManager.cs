using Godot;
using SakugaEngine.Resources;
using System.IO;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class StanceManager : Node
    {
        private SakugaFighter owner;
        [Export] public int DefaultStance = 0;
        [Export] public FighterStance[] Stances;

        public int CurrentStance;
        public int BufferedMove = -1;
        public int CurrentMove = -1;
        public bool CanMoveCancel;

        private ushort buttonChargeState;

        public void Initialize(SakugaFighter owner)
        {
            this.owner = owner;
            CurrentStance = DefaultStance;
        }

        public bool CheckMoveConditions(int index)
        {
            if (owner.Body.ContainsFrameProperty((byte)Global.FrameProperties.LOCK_MOVE)) return false;

            if (CurrentMove >= 0)
            {
                if (CurrentMove == index)
                {
                    bool canOverride = !GetCurrentMove().CanBeOverrided || (GetCurrentMove().CanBeOverrided && GetCurrentMove().CanOverrideToSelf);

                    if (!canOverride) return false;
                }

                if (GetMove(index).PriorityBuffer && GetMove(index).Priority < GetCurrentMove().Priority) return false;
            }

            int distance = Global.Distance(owner.GetOpponent().Body.FixedPosition, owner.Body.FixedPosition).X;
            bool isValidDistance = distance >= GetMove(index).DistanceArea.X && distance <= GetMove(index).DistanceArea.Y;
            if (!isValidDistance) return false;

            bool isCorrectSurface = (GetMove(index).UseOnGround && owner.Body.IsOnGround) ||
                (GetMove(index).UseOnAir && !owner.Body.IsOnGround) ||
                (GetMove(index).UseOnGround && GetMove(index).UseOnAir);
            if (!isCorrectSurface) return false;

            bool isValidState = CheckStatesToIgnore(index);
            if (!isValidState) return false;

            bool isCorrectState = CheckOwnerState(index);
            if (!isCorrectState) return false;

            bool isDesiredHealth = owner.Variables.CurrentHealth >= GetMove(index).HealthThreshold.X &&
                owner.Variables.CurrentHealth <= GetMove(index).HealthThreshold.Y;
            if (!isDesiredHealth) return false;
            
            if (owner.Variables.CurrentSuperGauge < GetMove(index).SuperGaugeRequired) return false;

            if (!owner.Variables.CompareExtraVariables(GetMove(index).ExtraVariablesRequirement)) return false;
            
            bool AllowedFrameWindow = owner.Animator.GetCurrentState().Loop || !owner.Animator.GetCurrentState().Loop && owner.Animator.Frame < GetMove(index).FrameLimit;
            if (GetMove(index).FrameLimit > 0 && !AllowedFrameWindow) return false;
            
            return true;
        }

        public void CheckMoves()
        {
            if (owner.Animator.StateType() > 3) return;

            if (CurrentMove >= 0)
                owner.Variables.AddSuperGauge(GetCurrentMove().BuildSuperGauge);

            for (int i = GetMoveListLength() - 1; i >= 0; i--)
            {
                if (!CheckMoveConditions(i)) continue;
                if (owner.Inputs.CheckMotionInputs(GetMove(i).Inputs))
                {
                    BufferedMove = i;
                    owner.MoveBuffer.Start();
                    break;
                }
            }
            
            AttackBufferStorage();
            ChargeButtonSequence();
            CheckMoveEndCondition();
        }

        public void ExecuteMove()
        {
            if (GetMove(BufferedMove).InterruptCornerPushback) owner.StopPushing();
            owner.Variables.CurrentSuperGauge -= GetMove(BufferedMove).SuperGaugeRequired;

            if (owner.Variables.CurrentHealth > 10 && GetMove(BufferedMove).SpendHealth > 0)
                owner.Variables.CurrentHealth -= GetMove(BufferedMove).SpendHealth;

            if (GetMove(BufferedMove).SuperFlash > 0 && !owner.SuperFlash)
            {
                owner.GetOpponent().SuperFlash = true;
                owner.GetOpponent().HitStop.Start((uint)GetMove(BufferedMove).SuperFlash);
            }

            if (GetMove(BufferedMove).MoveState >= 0) 
                owner.Animator.PlayState(GetMove(BufferedMove).MoveState, true);
            
            owner.Variables.ExtraVariablesOnMoveEnter();
            
            owner.Variables.SetExtraVariables(GetMove(BufferedMove).ExtraVariablesChange);

            CurrentMove = BufferedMove;
            BufferedMove = -1;

            CanMoveCancel = false;

            GD.Print("You executed " + GetCurrentMove().MoveName + "!");
        }

        public void AttackBufferStorage()
        {
            if (BufferedMove >= 0)
            {
                if (!owner.MoveBuffer.IsRunning() && !owner.SuperFlash)
                {
                    BufferedMove = -1;
                    GD.Print("Buffer Cleaned!");
                }
                else if (!owner.HitStop.IsRunning())
                {
                    bool CanOverride = CurrentMove < 0 || (GetCurrentMove().CanBeOverrided && 
                                        (GetCurrentMove().IgnoreSamePriority ? 
                                        GetMove(BufferedMove).Priority > GetCurrentMove().Priority : 
                                        GetMove(BufferedMove).Priority >= GetCurrentMove().Priority));
                    
                    bool allowMoveCancel = CanMoveCancel || owner.Body.ContainsFrameProperty((byte)Global.FrameProperties.FORCE_MOVE_CANCEL);
                    
                    bool canCancelThis = (allowMoveCancel && CanCancel()) || 
                                (owner.Animator.Frame < Global.KaraCancelWindow && CanKaraCancel());
                                        
                    bool isValidStateType = GetMove(BufferedMove).WaitForNullStates ? 
                                            (owner.Animator.GetCurrentState().Type > Global.StateType.NULL &&
                                            owner.Animator.GetCurrentState().Type <= Global.StateType.COMBAT) :
                                            owner.Animator.GetCurrentState().Type <= Global.StateType.COMBAT;
                    
                    if (isValidStateType && (CanOverride || canCancelThis))
                    {
                        ExecuteMove();
                        //GD.Print("You executed " + GetCurrentMove().MoveName + "!");
                    }
                    else GD.Print("Move " + GetMove(BufferedMove).MoveName + " Buffered!");
                }
            }
        }

        private void ChargeButtonSequence()
        {
            if (CurrentMove < 0) return;
            if (GetCurrentMove().buttonChargeSequence.Length == 0) return;

            if (owner.Inputs.CheckInputEnd(GetCurrentMove().Inputs))
            {
                ButtonChargeSequence currSequence;
                for (int i = 0; i < GetCurrentMove().buttonChargeSequence.Length; i++)
                {
                    currSequence = GetCurrentMove().buttonChargeSequence[i];
                    if (Global.IsOnRange((int)buttonChargeState, currSequence.Threshold.X, currSequence.Threshold.Y))
                    {
                        if (CheckMoveConditions(currSequence.SequenceMove))
                        {
                            CurrentMove = -1;
                            BufferedMove = currSequence.SequenceMove;
                            ExecuteMove();
                        }
                    }
                }
            }
            else
            {
                if (owner.Inputs.CurrentInput().bCharge > 0) buttonChargeState = owner.Inputs.CurrentInput().bCharge;

                ButtonChargeSequence limitSequence = GetCurrentMove().buttonChargeSequence[GetCurrentMove().buttonChargeSequence.Length - 1];

                if (owner.Inputs.InputBufferDuration() > limitSequence.Threshold.Y)
                {
                    if (CheckMoveConditions(limitSequence.SequenceMove))
                    {
                        CurrentMove = -1;
                        BufferedMove = limitSequence.SequenceMove;
                        ExecuteMove();
                    }
                }
            }
        }

        private void CheckMoveEndCondition()
        {
            if (CurrentMove < 0) return;

            if ((int)GetCurrentMove().MoveEnd == 0 && owner.Animator.CurrentState != GetCurrentMove().MoveState ||
                (int)GetCurrentMove().MoveEnd == 1 && owner.Inputs.CheckInputEnd(GetCurrentMove().Inputs) ||
                (int)GetCurrentMove().MoveEnd == 2 && owner.Animator.StateType() != (int)owner.Animator.States[GetCurrentMove().MoveState].Type)
            {
                ResetStance();
            }
        }

        public void ResetStance()
        {
            if (GetCurrentMove().MoveEndState >= 0)
                owner.Animator.PlayState(GetCurrentMove().MoveEndState, false);
            
            if (CanMoveCancel) CanMoveCancel = false;
            
            if (GetCurrentMove().ChangeStance >= 0)
            {
                CurrentStance = GetCurrentMove().ChangeStance;
                BufferedMove = -1;
            }
            CurrentMove = -1;
            owner.Variables.ExtraVariablesOnMoveExit();
        }

        public void Clear()
        {
            CurrentMove = -1;
            CanMoveCancel = false;
            if (!GetCurrentStance().IsDamagePersistent) CurrentStance = 0;
        }

        public bool CheckOwnerState(int index)
        {
            if (GetMove(index).IsSequenceFromStates == null) return true;
            if (GetMove(index).IsSequenceFromStates.Length <= 0) return true;
            
            foreach (int possibleStates in GetMove(index).IsSequenceFromStates)
            {
                if (owner.Animator.CurrentState == possibleStates)
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
                if (owner.Animator.CurrentState == possibleStates)
                    return false;
            }
            return true;
        }

        public bool CanCancel()
        {
            if (CurrentMove < 0) return false;
            if (GetCurrentMove().CancelsTo == null) return false;
            if (GetCurrentMove().CancelsTo.Length <= 0) return false;

            foreach (int possibleCancel in GetCurrentMove().CancelsTo)
            {
                if (BufferedMove == possibleCancel)
                    return true;
            }
            return false;
        }

        public bool CanKaraCancel()
        {
            if (CurrentMove < 0) return false;
            if (GetCurrentMove().KaraCancelsTo == null) return false;
            if (GetCurrentMove().KaraCancelsTo.Length <= 0) return false;

                foreach (int possibleKaraCancel in GetCurrentMove().KaraCancelsTo)
                {
                    if (BufferedMove == possibleKaraCancel)
                        return true;
                }
            return false;
        }

        public FighterStance GetCurrentStance() => Stances[CurrentStance];
        public MoveSettings GetMove(int index) => GetCurrentStance().Moves[index];
        public MoveSettings GetCurrentMove() => GetMove(CurrentMove);
        public int GetMoveListLength() => GetCurrentStance().Moves.Length;
        public bool CanAutoTurn() => CurrentMove < 0 || (int)GetCurrentMove().SideChange > 0;

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(CurrentStance);
            bw.Write(BufferedMove);
            bw.Write(CurrentMove);
            bw.Write(CanMoveCancel);
        }

        public void Deserialize(BinaryReader br)
        {
            CurrentStance = br.ReadInt32();
            BufferedMove = br.ReadInt32();
            CurrentMove = br.ReadInt32();
            CanMoveCancel = br.ReadBoolean();
        }
    }
}