using Godot;
using SakugaEngine.Resources;
using System;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class AIBrain : Node
    {
        private SakugaFighter _owner;
        [Export] public AIAction[] Actions;
        [Export] public int HighBlockAction;
        [Export] public int LowBlockAction;
        [Export] public int ForwardRecoveryAction;
        [Export] public int BackRecoveryAction;
        [Export] public int ThrowEscapeAction;

        [ExportCategory("Behaviors")]
        [Export] private AIBehavior BehaviorBeginner;
        [Export] private AIBehavior BehaviorEasy;
        [Export] private AIBehavior BehaviorMedium;
        [Export] private AIBehavior BehaviorHard;
        [Export] private AIBehavior BehaviorVeryHard;
        [Export] private AIBehavior BehaviorPro;

        private Global.BotMode mode = Global.BotMode.AGGRESSIVE;
        private AIBehavior currentBehavior;
        private int tick = 0;
        private int tickLimit = 1;
        private int inputTick = 0;
        private int inputTickLimit = 1;
        private int[] currentCommandList = new int[1];
        private int currentCommand = 0;
        private int currentInputIndex = 0;
        private int currentDecisionRate;
        public bool canAdvance;
        public bool inputFinished = true;
        public bool blocking;
        public bool teching;
        public Global.HitType proximityHit;

        public void Initialize(SakugaFighter owner)
        {
            _owner = owner;

            switch (Global.Match.botDifficulty)
            {
                case Global.BotDifficulty.BEGINNER:
                    currentBehavior = BehaviorBeginner;
                    break;
                case Global.BotDifficulty.EASY:
                    currentBehavior = BehaviorEasy;
                    break;
                case Global.BotDifficulty.MEDIUM:
                    currentBehavior = BehaviorMedium;
                    break;
                case Global.BotDifficulty.HARD:
                    currentBehavior = BehaviorHard;
                    break;
                case Global.BotDifficulty.VERY_HARD:
                    currentBehavior = BehaviorVeryHard;
                    break;
                case Global.BotDifficulty.PRO:
                    currentBehavior = BehaviorPro;
                    break;
            }

            currentDecisionRate = 60;
        }

        private int UpdateDecisionRateFree()
        {
            return Global.RNG.Next(currentBehavior.DecisionRateFree.X, currentBehavior.DecisionRateFree.Y + 1);
        }

        private int UpdateDecisionRateBusy()
        {
            return Global.RNG.Next(currentBehavior.DecisionRateBusy.X, currentBehavior.DecisionRateBusy.Y + 1);
        }

        private int SetInputRandomWaitTime()
        {
            return Global.RNG.Next(currentBehavior.InputRandomness.X, currentBehavior.InputRandomness.Y + 1);
        }

        public void Reset()
        {
            if (!_owner.Body.ProximityBlocked)
            {
                currentCommandList = new int[1];
                blocking = false;
            }
            currentCommand = 0;
            currentInputIndex = 0;
            teching = false;
            canAdvance = false;
            inputFinished = true;
        }

        public void SelectCommand()
        {
            //Ignore AI behavior if the round ended
            if (_owner.LifeEnded()) return;
            if (_owner.GetOpponent().LifeEnded()) return;

            //if ((currentCommand > 0 || currentInputIndex > 0 && !canAdvance) && _owner.Stance.CurrentMove < 0)
            //{ Reset(); }
            //bool MoveEnded = (currentCommand == 0 || currentCommand >= currentCommandList.Length) && currentInputIndex == 0;
            //if (!MoveEnded) { tick = 0; return; }

            //Reset the values if the character is busy
            if (_owner.Animator.StateType() > 1)
            {
                if (_owner.Animator.StateType() == 4)
                {
                    if (!teching)
                    {
                        if (_owner.IsGrabbed() && _owner.HitStop.IsRunning())
                        {
                            int rnd = Global.RNG.Next(0, 10);
                            if (rnd < currentBehavior.TechingRate)
                                currentCommandList = new int[] { ThrowEscapeAction };

                            teching = true;
                        }
                        else if (_owner.HitStun.IsRunning())
                        {
                            int rnd = Global.RNG.Next(0, 10);
                            if (rnd < currentBehavior.TechingRate)
                            {
                                rnd = Global.RNG.Next(0, 10);
                                currentCommandList = new int[1];
                                switch (mode)
                                {
                                    default:
                                        break;
                                    case Global.BotMode.AGGRESSIVE:
                                        if (rnd < currentBehavior.TechingRate)
                                            currentCommandList[0] = ForwardRecoveryAction;
                                        else
                                            currentCommandList[0] = BackRecoveryAction;
                                        break;
                                    case Global.BotMode.DEFENSIVE:
                                        if (rnd < currentBehavior.TechingRate)
                                            currentCommandList[0] = BackRecoveryAction;
                                        else
                                            currentCommandList[0] = ForwardRecoveryAction;
                                        break;
                                }
                                
                                
                            }
                            teching = true;
                        }
                        
                        return;
                    }
                }
                if (_owner.Animator.StateType() > 2) Reset();

                tick = 0;
                return;
            }
            else if (_owner.Body.ProximityBlocked) //And allow them to defend themselves if free
            {
                if (!blocking)
                {
                    Reset();
                    int rnd = Global.RNG.Next(0, 10);
                    if (rnd < currentBehavior.BlockingRate)
                    {
                        int blockStance = proximityHit == Global.HitType.LOW ? LowBlockAction : HighBlockAction;
                        currentCommandList = new int[] { blockStance };
                    }

                    blocking = true;
                }
                tick = 0;
                tickLimit = 1;
                inputTick = 0;
                inputTickLimit = 1;
                return;
            }

            //If a command is still running, let it do its thing
            if ((currentCommand >= currentCommandList.Length || (currentCommand >= 0 && inputFinished && !canAdvance
                && !Actions[currentCommandList[currentCommand]].AutoAdvance)) && _owner.Stance.CurrentMove < 0)
            { Reset(); }
            bool MoveEnded = (currentCommand == 0 || currentCommand >= currentCommandList.Length) && currentInputIndex == 0;
            //GD.Print($"{currentCommand == 0 || currentCommand >= currentCommandList.Length}, {currentInputIndex == 0}");
            if (!MoveEnded) { tick = 0; return; }

            //Advance decision tick
            //Select a different decision range depending on the character's availability
            tick++;
            if (tick <= tickLimit) return;
            else
            {
                if (_owner.Stance.CurrentMove < 0)
                    tickLimit = UpdateDecisionRateFree();
                else
                    tickLimit = UpdateDecisionRateBusy();

                tick = 0;
            }

            //Store previous values
            int[] previousCommandList = currentCommandList;
            int previousCommand = currentCommand;
            int previousInputIndex = currentInputIndex;

            Reset();

            Vector2I distance = Global.Distance(_owner.Body.FixedPosition, _owner.GetOpponent().Body.FixedPosition);

            //Select command pack based on distance
            AIActionPack selectedList = currentBehavior.NearActions;
            if (distance.X > currentBehavior.NearActions.HorizontalDistance &&
                distance.X <= currentBehavior.MidActions.HorizontalDistance)
                selectedList = currentBehavior.MidActions;
            else if (distance.X > currentBehavior.MidActions.HorizontalDistance &&
                distance.X <= currentBehavior.FarActions.HorizontalDistance)
                selectedList = currentBehavior.FarActions;
            else if (distance.X > currentBehavior.FarActions.HorizontalDistance)
                selectedList = currentBehavior.DistantActions;

            mode = _owner.Variables.CurrentHealth < currentBehavior.LowHealth ? Global.BotMode.DEFENSIVE : Global.BotMode.AGGRESSIVE;

            //Select a move right after
            if (selectedList.Conditions.Length == 1)
            {
                currentCommandList = selectedList.Conditions[0].ActionsList;
            }
            else
            {
                int prevDistX = 0;
                int prevDistY = 0;
                bool movePredicted = false;
                for (int i = 0; i < selectedList.Conditions.Length; i++)
                {
                    //Check correct surface
                    bool correctSurface = (selectedList.Conditions[i].UseOnGround && _owner.Body.IsOnGround) ||
                                        (selectedList.Conditions[i].UseOnAir && !_owner.Body.IsOnGround) ||
                                        (selectedList.Conditions[i].UseOnGround && selectedList.Conditions[i].UseOnAir);
                    if (!correctSurface) continue;

                    //Check super gauge
                    bool enoughSuperGauge = selectedList.Conditions[i].SuperGaugeRequired <= _owner.Variables.CurrentSuperGauge;
                    if (!enoughSuperGauge) continue;

                    //Check distance for each command
                    bool isSameX = selectedList.Conditions[i].Distance.X == prevDistX;
                    bool ignoreY = selectedList.Conditions[i].Distance.Y == 0;
                    bool checkFar = (isSameX || distance.X > prevDistX) && (ignoreY || distance.Y >= prevDistY);
                    bool checkClose = distance.X <= selectedList.Conditions[i].Distance.X &&
                                                (ignoreY || distance.Y <= selectedList.Conditions[i].Distance.Y);
                    if (!checkFar || !checkClose) continue;

                    //Allow the CPU to react according to the flags
                    if (currentBehavior.PredictionQuality > 0)
                    {
                        if ((selectedList.Conditions[i].CounterFlags & _owner.Animator.GetCurrentState().AIFlags) != 0)
                        {
                            int rnd = Global.RNG.Next(0, 10);
                            if (rnd <= currentBehavior.PredictionQuality - 1)
                            {
                                currentCommandList = selectedList.Conditions[i].ActionsList;
                                movePredicted = true;
                            }
                        }
                    }
                    if (movePredicted) continue;

                    //If failed, select a new move randomly
                    if (i > 0)
                    {
                        if (selectedList.Conditions[i].Probability == 0) continue;

                        int probability = VariableProbability(selectedList.Conditions[i]);
                        int rnd = Global.RNG.Next(0, 10);
                        if (rnd > probability - 1) continue;
                    }

                    currentCommandList = selectedList.Conditions[i].ActionsList;
                    prevDistX = selectedList.Conditions[i].Distance.X;
                    prevDistY = selectedList.Conditions[i].Distance.Y;
                }
            }

            //If the new move is the same as the previous one, just keep moving
            if (CompareCommands(currentCommandList, previousCommandList))
            {
                currentCommand = previousCommand;
                currentInputIndex = previousInputIndex;
            }

            inputTick = 0;
            inputTickLimit = SetInputRandomWaitTime();
        }

        public void UpdateCommand()
        {
            if (currentCommandList == null || currentCommandList.Length <= 0)
            { _owner.ParseInputs(0); return; }

            if (currentCommand >= currentCommandList.Length)
            { _owner.ParseInputs(0); return; }

            int curr = currentCommandList[currentCommand];
            if (curr < 0) { _owner.ParseInputs(0); return; }

            inputTick++;
            inputFinished = currentInputIndex >= Actions[curr].Inputs.Length - 1;
            _owner.ParseInputs(GenerateInput(Actions[curr].Inputs[currentInputIndex]));

            if (inputTick <= inputTickLimit) return;
            else
            {
                inputTickLimit = SetInputRandomWaitTime();
                inputTick = 0;
            }
            currentInputIndex++;
            currentInputIndex = Mathf.Clamp(currentInputIndex, 0, Actions[curr].Inputs.Length - 1);

            if (inputFinished && (canAdvance || Actions[curr].AutoAdvance) && currentCommand < currentCommandList.Length)
            {
                currentCommand++;
                currentInputIndex = 0;
                canAdvance = false;
            }
        }

        private int VariableProbability(AICondition condition)
        {
            int prob = 0;
            switch (mode)
            {
                default:
                    break;
                case Global.BotMode.AGGRESSIVE:
                    switch (condition.ActionMode)
                    {
                        default:
                            break;
                        case Global.BotMode.AGGRESSIVE:
                            prob += 1;
                            break;
                        case Global.BotMode.DEFENSIVE:
                            prob -= 2;
                            break;
                    }
                    break;
                case Global.BotMode.DEFENSIVE:
                    switch (condition.ActionMode)
                    {
                        default:
                            break;
                        case Global.BotMode.AGGRESSIVE:
                            prob -= 2;
                            break;
                        case Global.BotMode.DEFENSIVE:
                            prob += 1;
                            break;
                    }
                    break;
            }

            int result = condition.Probability + prob;
            if (result > 10) result = 10;
            else if (result < 0) result = 0;

            return result;
        }

        private bool CompareCommands(int[] c1, int[] c2)
        {
            if (c1 == null || c2 == null) return false;
            if (c1.Length == 0 || c2.Length == 0) return false;
            if (c1.Length != c2.Length) return false;

            for (int i = 0; i < c1.Length; i++)
            {
                if (c1[i] != c2[i]) return false;
            }

            return true;
        }

        private ushort GenerateInput(AIInput input)
        {
            int result = 0;

            bool side = _owner.Body.IsLeftSide;

            int left = side ? Global.INPUT_LEFT : Global.INPUT_RIGHT;
            int right = side ? Global.INPUT_RIGHT : Global.INPUT_LEFT;

            // Directional inputs
            if ((input.Direction & Global.DirectionalInputs.UP) > 0)
                result |= Global.INPUT_UP;
            if ((input.Direction & Global.DirectionalInputs.DOWN) > 0)
                result |= Global.INPUT_DOWN;
            if ((input.Direction & Global.DirectionalInputs.LEFT) > 0)
                result |= left;
            if ((input.Direction & Global.DirectionalInputs.RIGHT) > 0)
                result |= right;
            
            // Face button inputs
            if ((input.Buttons & Global.ButtonInputs.FACE_A) > 0)
                result |= Global.INPUT_FACE_A;
            if ((input.Buttons & Global.ButtonInputs.FACE_B) > 0)
                result |= Global.INPUT_FACE_B;
            if ((input.Buttons & Global.ButtonInputs.FACE_C) > 0)
                result |= Global.INPUT_FACE_C;
            if ((input.Buttons & Global.ButtonInputs.FACE_D) > 0)
                result |= Global.INPUT_FACE_D;

            return (ushort)result;
        }
    }
}
