using Godot;
using System.IO;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class StanceManager : Node
    {
        private FighterBody owner;
        [Export] public int DefaultStance = 0;
        [Export] public FighterStance[] Stances;

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

            bool isDesiredHealth = owner.Variables.CurrentHealth >= GetMove(index).HealthThreshold.X &&
                owner.Variables.CurrentHealth <= GetMove(index).HealthThreshold.Y;
            if (!isDesiredHealth) return false;
            
            if (owner.Variables.CurrentSuperGauge < GetMove(index).SuperGaugeRequired) return false;

            if (!owner.Variables.CompareExtraVariables(GetMove(index).ExtraVariablesRequirement)) return false;
            
            return true;
        }

        public void CheckMoves()
        {
            if (owner.Animator.StateType() > 3) return;

            for (int i = GetMoveListLength() - 1; i >= 0; i--)
            {
                if (!CheckMoveConditions(i)) continue;
                if (owner.Inputs.CheckMotionInputs(GetMove(i).Inputs))
                {
                    bufferedMove = i;
                    owner.MoveBuffer.Start();
                    break;
                }
            }
            CheckMoveEndCondition();
            AttackBufferStorage();
        }

        public void ExecuteMove()
        {
            owner.Variables.CurrentSuperGauge -= (uint)GetMove(bufferedMove).SuperGaugeRequired;

            if (owner.Variables.CurrentHealth > 10 && GetMove(bufferedMove).SpendHealth > 0)
                owner.Variables.CurrentHealth -= (uint)GetMove(bufferedMove).SpendHealth;

            if (GetMove(bufferedMove).SuperFlash > 0 && !owner.SuperStop)
            {
                owner.GetOpponent().SuperStop = true;
                owner.GetOpponent().HitStop.Start((uint)GetMove(bufferedMove).SuperFlash);
            }

            owner.CallState(GetMove(bufferedMove).MoveState, true);
            owner.Variables.SetExtraVariables(GetMove(bufferedMove).ExtraVariablesChange);

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
                    bool CanOverride = currentMove == -1 || (GetCurrentMove().CanBeOverrided && 
                                        (GetCurrentMove().IgnoreSamePriority ? 
                                        GetMove(bufferedMove).Priority > GetCurrentMove().Priority : 
                                        GetMove(bufferedMove).Priority >= GetCurrentMove().Priority));
                    
                    bool canCancelThis = (canMoveCancel && CanCancel()) || 
                                (owner.Animator.Frame < Global.KaraCancelWindow && CanKaraCancel());
                    
                    if (CanOverride || canCancelThis)
                    {
                        ExecuteMove();
                        GD.Print("You executed " + GetCurrentMove().MoveName + "!");
                    }
                    else GD.Print("Move " + GetMove(bufferedMove).MoveName + " Buffered!");
                }
            }
        }

        private void CheckMoveEndCondition()
        {
            if (currentMove < 0) return;

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
                owner.CallState(GetCurrentMove().MoveEndState, false);
            
            if (canMoveCancel) canMoveCancel = false;
            
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
                if (owner.Animator.CurrentState == possibleStates)
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

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(CurrentStance);
            bw.Write(bufferedMove);
            bw.Write(currentMove);
            bw.Write(canMoveCancel);
        }

        public void Deserialize(BinaryReader br)
        {
            CurrentStance = br.ReadInt32();
            bufferedMove = br.ReadInt32();
            currentMove = br.ReadInt32();
            canMoveCancel = br.ReadBoolean();
        }
    }
}

public struct MoveButtons
{
    public int ButtonPressedTime;
    public int CurrentButtonIndex;
};
