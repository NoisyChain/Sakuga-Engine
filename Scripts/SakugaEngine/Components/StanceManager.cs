using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class StanceManager : Node
    {
        private FighterBody owner;
        [Export] public int DefaultStance = 0;
        [Export] public FighterStance[] Stances;
        [Export] public int[] HitReactions;
        public int CurrentStance;
        public int bufferedMove = -1;
        public int currentMove = -1;
        public bool canMoveCancel;

        public void Initialize(FighterBody owner)
        {
            this.owner = owner;
            CurrentStance = DefaultStance;
        }

        public bool CheckMoveConditions(int index)
        {
            if (currentMove >= 0 && currentMove == index)
            {
                bool canOverride = GetCurrentMove().CanBeOverrided && GetCurrentMove().CanOverrideToSelf;

                if (!canOverride) return false;
            }

            bool isCorrectSurface = (GetMove(index).UseOnGround && owner.Body.IsOnGround) ||
                (GetMove(index).UseOnAir && !owner.Body.IsOnGround) ||
                (GetMove(index).UseOnGround && GetMove(index).UseOnAir);
            if (!isCorrectSurface) return false;

            bool isCorrectState = GetMove(index).IsSequenceFromStates.Length <= 0 || CheckOwnerState(index);
            if (!isCorrectState) return false;

            //bool isOpponentCorrectState = GetMove(index).CheckOpponentStates.Length <= 0 || CheckOpponentState(index);
            //if (!isOpponentCorrectState) return false;

            bool isDesiredHealth = owner.Variables.CurrentHealth >= GetMove(index).MinimumHealth &&
                owner.Variables.CurrentHealth <= GetMove(index).MaximumHealth;
            if (!isDesiredHealth) return false;
            
            if (owner.Variables.CurrentSuperGauge < GetMove(index).SuperGaugeRequired) return false;
            
            return true;
        }

        public void CheckMoves()
        {
            if (owner.StateType() > 3) return;

            for (int i = GetMoveListLength() - 1; i >= 0; i--)
            {
                if (!CheckMoveConditions(i)) continue;
                for(int j = 0; j < GetMove(i).ValidInputs.Length; j++)
                {
                    if (owner.Inputs.CheckMotionInputs(GetMove(i).ValidInputs[j]))
                    {
                        bufferedMove = i;
                        owner.MoveBuffer.Start();
                        break;
                    }
                }
            }
            CheckMoveEndConndition();
            AttackBufferStorage();
        }

        public void ExecuteMove(int moveIndex)
        {
            owner.Variables.CurrentSuperGauge -= (uint)GetMove(moveIndex).SuperGaugeRequired;

            if (owner.Variables.CurrentHealth > 10 && GetMove(moveIndex).SpendHealth > 0)
                owner.Variables.CurrentHealth -= (uint)GetMove(moveIndex).SpendHealth;

            if (GetMove(moveIndex).SuperFlash > 0 && !owner.SuperStop)
            {
                //owner.GetOpponent().SuperStop = true;
                //owner.GetOpponent().HitStop = GetMove(moveIndex).TimeFreeze;
            }

            owner.CallState(GetMove(moveIndex).MoveState, true);
            //owner.ForceMovementMode();

            currentMove = bufferedMove;
            bufferedMove = -1;

            canMoveCancel = false;
        }

        public void AttackBufferStorage()
        {
            if (bufferedMove >= 0)
            {
                if (!owner.MoveBuffer.IsRunning() && !owner.SuperStop)
                {
                    bufferedMove = -1;
                    GD.Print("Buffer Cleaned!");
                }
                else if (!owner.HitStop.IsRunning())
                {
                    bool CanOverride = currentMove == -1 || 
                                GetCurrentMove().CanBeOverrided && 
                                GetMove(bufferedMove).Priority > GetCurrentMove().Priority;
                    
                    bool canCancelThis = (canMoveCancel && CanCancel()) || 
                                (owner.Animator.Frame < Global.KaraCancelWindow && CanKaraCancel());
                    
                    if (CanOverride || canCancelThis)
                    {
                        ExecuteMove(bufferedMove);
                        GD.Print("You executed " + GetCurrentMove().MoveName + "!");
                    }
                    else GD.Print("Move " + GetMove(bufferedMove).MoveName + " Buffered!");
                }
            }

            if (canMoveCancel && owner.StateType() != 3) canMoveCancel = false;
        }

        private void CheckMoveEndConndition()
        {
            if (currentMove < 0) return;

            if ((int)GetCurrentMove().MoveEnd == 0 && owner.CurrentState != GetCurrentMove().MoveState ||
                (int)GetCurrentMove().MoveEnd == 1 && owner.Inputs.CheckInputEnd(GetCurrentMove().ValidInputs[0]) ||
                (int)GetCurrentMove().MoveEnd == 2 && owner.StateType() != (int)owner.States[GetCurrentMove().MoveState].Type)
            {
                ResetStance();
            }
        }

        public void ResetStance()
        {
            if (GetCurrentMove().MoveEndState >= 0)
                owner.CallState(GetCurrentMove().MoveEndState, false);
            
            if (GetCurrentMove().ChangeStance >= 0)
            {
                CurrentStance = GetCurrentMove().ChangeStance;
                bufferedMove = -1;
            }
            currentMove = -1;
        }

        public bool CheckOwnerState(int index) 
        {
            foreach (int possibleStates in GetMove(index).IsSequenceFromStates)
            {
                if (owner.CurrentState == possibleStates)
                    return true;
            }
            return false;
        }

        public bool CanCancel()
        {
            if (currentMove >= 0)
                foreach (int possibleCancel in GetCurrentMove().CancelsTo)
                {
                    if (GetCurrentMove().CancelsTo.Length > 0 && bufferedMove == possibleCancel)
                        return true;
                }
            return false;
        }

        public bool CanKaraCancel()
        {
            if (currentMove >= 0)
                foreach (int possibleKaraCancel in GetCurrentMove().KaraCancelsTo)
                {
                    if (GetCurrentMove().KaraCancelsTo.Length > 0 && bufferedMove == possibleKaraCancel)
                        return true;
                }
            return false;
        }

        public FighterStance GetCurrentStance() => Stances[CurrentStance];
        public MoveSettings GetMove(int index) => GetCurrentStance().Moves[index];
        public MoveSettings GetCurrentMove() => GetMove(currentMove);
        public int GetMoveListLength() => GetCurrentStance().Moves.Length;
        public bool CanAutoTurn() => currentMove < 0 || (int)GetCurrentMove().SideChange > 0;
    }
}

public struct MoveButtons
{
    public int ButtonPressedTime;
    public int CurrentButtonIndex;
};
