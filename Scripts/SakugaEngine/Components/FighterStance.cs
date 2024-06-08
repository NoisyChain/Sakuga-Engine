using Godot;
using System;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterStance : Node
    {
        private FighterBody owner;
        [Export] public int NeutralState = 0;
        [Export] public MoveSettings[] Moves;
        [Export] public int[] HitReactions;
        public int checkedMove = -1;
        public int bufferedMove = -1;
        public int currentMove = -1;
        public bool canMoveCancel;
        public int MoveBuffer;

        public void Initialize(FighterBody owner)
        {
            this.owner = owner;
        }

        public bool CheckMoveConditions(int index)
        {
            bool isCorrectSurface = GetMove(index).UseOnGround && owner.IsOnGround ||
                (GetMove(index).UseOnAir && !owner.IsOnGround) ||
                (GetMove(index).UseOnGround && GetMove(index).UseOnAir);
            if (!isCorrectSurface) return false; 

            //bool isCorrectState = GetMove(index).IsSequenceFromStates.Length <= 0 || CheckOwnerState(index);
            //if (!isCorrectState) return false;

            //bool isOpponentCorrectState = GetMove(index).CheckOpponentStates.Length <= 0 || CheckOpponentState(index);
            //if (!isOpponentCorrectState) return false;

            bool isDesiredHealth = owner.Variables.CurrentHealth >= GetMove(index).MinimumHealth &&
                owner.Variables.CurrentHealth <= GetMove(index).MaximumHealth;
            if (!isDesiredHealth) return false;
            
            if (owner.Variables.CurrentSuperGauge < GetMove(index).SuperGaugeRequired) return false;

            
            //attackBuffer = owner.GetSettings().AttackBuffer;
            //moveLockTimer = 1;
            
            return true;
        }

        public void CheckMoves()
        {
            for (int i = GetMoveListLength() - 1; i >= 0; i--)
            {
                if (owner.Inputs.CheckMotionInputs(GetMove(i).Command) && owner.StateType() != 3)
                {
                    if (CheckMoveConditions(i))
                    {
                        bufferedMove = i;
                        MoveBuffer = 10;
                        break;
                    }
                }
                else CheckMoveEndConndition();
            }

            AttackBufferStorage();
        }

        public void ExecuteMove(int moveIndex)
        {
            //if (currentMove >= 0 && owner.GetMove(currentMove).Persistent) return;

            //if (currentMove >= 0 && (int)owner.GetMove(currentMove).MoveEndCondition == 1 && currentMove == moveIndex) return;

            owner.Variables.CurrentSuperGauge -= (uint)GetMove(moveIndex).SuperGaugeRequired;

            if (owner.Variables.CurrentHealth > 10 && GetMove(moveIndex).SpendHealth > 0)
                owner.Variables.CurrentHealth -= (uint)GetMove(moveIndex).SpendHealth;

            if (GetMove(moveIndex).ChangeStance > -1)
                owner.CurrentStance = GetMove(moveIndex).ChangeStance;

            /*if (GetMove(moveIndex).TimeFreeze > 0 && !owner.SuperStop)
            {
                owner.GetOpponent().SuperStop = true;
                owner.GetOpponent().HitStop = GetMove(moveIndex).TimeFreeze;
            }*/

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
                if (MoveBuffer <= 0 && !owner.SuperStop)
                {
                    bufferedMove = -1;
                    GD.Print("Buffer Cleaned!");
                }
                else if (owner.HitStop == 0)
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

            //if (canMoveCancel && owner.StateType() != 2) canMoveCancel = false;
        }

        private void CheckMoveEndConndition()
        {
            if (currentMove < 0) return;

            if ((int)GetMove(currentMove).MoveEnd == 0 && owner.CurrentState != GetMove(currentMove).MoveState ||
                (int)GetMove(currentMove).MoveEnd == 1 && owner.Inputs.IsNeutral() ||
                (int)GetMove(currentMove).MoveEnd == 2 && owner.StateType() != (int)owner.States[GetMove(currentMove).MoveState].Type)
            {
                if (GetMove(currentMove).MoveEndState >= 0)
                    owner.CallState(GetMove(currentMove).MoveEndState, false);
                currentMove = -1;
            }
        }

        public bool CanCancel()
        {
            if (currentMove >= 0)
                foreach (int possibleCancel in GetMove(currentMove).CancelsTo)
                {
                    if (GetMove(currentMove).CancelsTo.Length > 0 && bufferedMove == possibleCancel)
                        return true;
                }
            return false;
        }

        public bool CanKaraCancel()
        {
            if (currentMove >= 0)
                foreach (int possibleKaraCancel in GetMove(currentMove).KaraCancelsTo)
                {
                    if (GetMove(currentMove).KaraCancelsTo.Length > 0 && bufferedMove == possibleKaraCancel)
                        return true;
                }
            return false;
        }

        public MoveSettings GetMove(int index) => Moves[index];
        public MoveSettings GetCurrentMove() => GetMove(currentMove);
        public int GetMoveListLength() => Moves.Length;
    }
}

public struct MoveButtons
{
    public int ButtonPressedTime;
    public int CurrentButtonIndex;
};
