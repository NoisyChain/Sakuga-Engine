using Godot;
using SakugaEngine.Resources;
using SakugaEngine.Global;

namespace SakugaEngine
{
    public class InputManager
    {
        public InputRegistry[] InputHistory;
        public short hCharge;
        public short vCharge;
        public int CurrentHistory;

        public InputManager()
        {
            InputHistory = new InputRegistry[GlobalVariables.InputHistorySize];
            hCharge = 0;
            vCharge = 0;
            CurrentHistory = 0;
        }

        public bool CheckMotionInputs(MotionInputs motion, int side)
        {
            if (motion == null) return false;
            if (motion.ValidInputs == null) return false;
            
            bool inputFound = false;

            for (int i = 0; i < motion.ValidInputs.Length; i++)
            {
                //Define the first input to check
                //If the result is less than 0, cycle it to the end
                int startingInput = (CurrentHistory - motion.ValidInputs[i].Inputs.Length) + 1;
                if (startingInput < 0) startingInput += GlobalVariables.InputHistorySize;

                for (int j = 0; j < motion.ValidInputs[i].Inputs.Length; j++)
                {
                    int HistoryIndex = (startingInput + j) % GlobalVariables.InputHistorySize;

                    DirectionalInputs directionals = motion.ValidInputs[i].Inputs[j].Directional;
                    ButtonInputs buttons = motion.ValidInputs[i].Inputs[j].Buttons;

                    ButtonMode dirMode = motion.ValidInputs[i].Inputs[j].DirectionalMode;
                    ButtonMode butMode = motion.ValidInputs[i].Inputs[j].ButtonMode;

                    bool validBuffer = motion.InputBuffer == 0 ||
                        InputHistory[HistoryIndex].duration <= motion.InputBuffer;

                    bool validInput;

                    if (directionals > 0 && buttons == 0)
                        validInput = CheckDirectionalInputs(HistoryIndex, side, directionals, dirMode, motion.AbsoluteDirection);
                    else if (directionals == 0 && buttons > 0)
                        validInput = CheckButtonInputs(HistoryIndex, buttons, butMode);
                    else
                        validInput = CheckDirectionalInputs(HistoryIndex, side, directionals, dirMode, motion.AbsoluteDirection) &&
                        CheckButtonInputs(HistoryIndex, buttons, butMode);
                        
                    bool chargedInput = CheckChargeInputs(HistoryIndex, side, directionals, motion.DirectionalChargeLimit);
                    
                    inputFound = (validBuffer && validInput) || chargedInput;
                    if (!inputFound) break;
                }
                if (inputFound) break;
            }
            
            return inputFound;
        }

        public bool CheckInputEnd(MotionInputs motion, int side)
        {
            DirectionalInputs directionals = motion.ValidInputs[0].Inputs[motion.ValidInputs[0].Inputs.Length - 1].Directional;
            ButtonInputs buttons = motion.ValidInputs[0].Inputs[motion.ValidInputs[0].Inputs.Length - 1].Buttons;

            bool validInput;

            if (directionals > 0 && buttons == 0)
                validInput = !CheckDirectionalInputs(CurrentHistory, side, directionals, ButtonMode.HOLD, motion.AbsoluteDirection);
            else if (directionals == 0 && buttons > 0)
                validInput = !CheckButtonInputs(CurrentHistory, buttons, ButtonMode.HOLD);
            else
                validInput = !CheckDirectionalInputs(CurrentHistory, side, directionals, ButtonMode.HOLD, motion.AbsoluteDirection) &&
                !CheckButtonInputs(CurrentHistory, buttons, ButtonMode.HOLD);

            if (!validInput)
                return false;
            
            return true;
        }

        public bool CheckDirectionalInputs(int index, int side, DirectionalInputs buttonNumber, ButtonMode buttonMode, bool absDirection)
        {
            PlayerInputs _left = PlayerInputs.LEFT;
            if (side < 0) _left = PlayerInputs.RIGHT;

            PlayerInputs _right = PlayerInputs.RIGHT;
            if (side < 0) _right = PlayerInputs.LEFT;

            bool left = GetButtonState(_left, index, buttonMode);
            bool right = GetButtonState(_right, index, buttonMode);
            bool up = GetButtonState(PlayerInputs.UP, index, buttonMode);
            bool down = GetButtonState(PlayerInputs.DOWN, index, buttonMode);

            bool absV = absDirection ? !up && !down : true;
            bool absH = absDirection ? !left && !right : true;

            bool notP = buttonMode == ButtonMode.NOT_PRESSED;
            bool canAbsH = notP || absH;
            bool canAbsV = notP || absV;
            bool neutralDirection = notP ? (down && up && left && right) : (!down && !up && !left && !right);
            
            return (buttonNumber == DirectionalInputs.DOWN && down && canAbsH) ||
                (buttonNumber == DirectionalInputs.LEFT && left && canAbsV) ||
                (buttonNumber == DirectionalInputs.RIGHT && right && canAbsV) ||
                (buttonNumber == DirectionalInputs.UP && up && canAbsH) ||
                (buttonNumber == DirectionalInputs.DOWN_LEFT && down && left) ||
                (buttonNumber == DirectionalInputs.DOWN_RIGHT && down && right) ||
                (buttonNumber == DirectionalInputs.UP_LEFT && up && left) ||
                (buttonNumber == DirectionalInputs.UP_RIGHT && up && right) ||
                (buttonNumber == 0 && neutralDirection);
        }

        public bool CheckButtonInputs(int index, ButtonInputs buttonNumber, ButtonMode buttonMode)
        {
            bool action_ba = GetButtonState(PlayerInputs.FACE_A, index, buttonMode);
            bool action_bb = GetButtonState(PlayerInputs.FACE_B, index, buttonMode);
            bool action_bc = GetButtonState(PlayerInputs.FACE_C, index, buttonMode);
            bool action_bd = GetButtonState(PlayerInputs.FACE_D, index, buttonMode);
            bool action_be = GetButtonState(PlayerInputs.FACE_E, index, buttonMode);
            bool action_bf = GetButtonState(PlayerInputs.FACE_F, index, buttonMode);
            bool action_bg = GetButtonState(PlayerInputs.FACE_G, index, buttonMode);
            bool action_bh = GetButtonState(PlayerInputs.FACE_H, index, buttonMode);

            bool canA = (buttonNumber & ButtonInputs.FACE_A) > 0;
            bool canB = (buttonNumber & ButtonInputs.FACE_B) > 0;
            bool canC = (buttonNumber & ButtonInputs.FACE_C) > 0;
            bool canD = (buttonNumber & ButtonInputs.FACE_D) > 0;
            bool canE = (buttonNumber & ButtonInputs.FACE_E) > 0;
            bool canF = (buttonNumber & ButtonInputs.FACE_F) > 0;
            bool canG = (buttonNumber & ButtonInputs.FACE_G) > 0;
            bool canH = (buttonNumber & ButtonInputs.FACE_H) > 0;

            return (buttonNumber == 0 && !action_ba && !action_bb && !action_bc && !action_bd && !action_be && !action_bf && !action_bg && !action_bh) ||
                (!canA || canA == action_ba) && (!canB || canB == action_bb) && (!canC || canC == action_bc) && (!canD || canD == action_bd) && 
                (!canE || canE == action_be) && (!canF || canF == action_bf) && (!canG || canG == action_bg) && (!canH || canH == action_bh);
        }

        public bool CheckChargeInputs(int index, int side, DirectionalInputs buttonNumber, int dirCharge)
        {
            if (dirCharge == 0) return false;

            bool _left = IsBeingPressed(index, PlayerInputs.LEFT);
            bool _right = IsBeingPressed(index, PlayerInputs.RIGHT);
            
            bool left = _left;
            if (side < 0) left = _right;

            bool right = _right;
            if (side < 0) right = _left;
            
            bool up = IsBeingPressed(index, PlayerInputs.UP);
            bool down = IsBeingPressed(index, PlayerInputs.DOWN);

            bool checkInputs = (buttonNumber == DirectionalInputs.LEFT && left && Mathf.Abs(hCharge) >= dirCharge) ||
                (buttonNumber == DirectionalInputs.DOWN && down && Mathf.Abs(vCharge) >= dirCharge) ||
                (buttonNumber == DirectionalInputs.DOWN_LEFT && left && Mathf.Abs(hCharge) >= dirCharge && down && vCharge <= -dirCharge) ||
                (buttonNumber == DirectionalInputs.UP_LEFT && left && Mathf.Abs(hCharge) >= dirCharge && up && vCharge >= dirCharge);

            return checkInputs;
        }

        public void InsertToHistory(PlayerInputs input)
        {
            if (InputHistory[CurrentHistory].rawInput != input)
            {
                CurrentHistory++;
                if (CurrentHistory >= GlobalVariables.InputHistorySize) CurrentHistory = 0;

                InputHistory[CurrentHistory].rawInput = input;
                InputHistory[CurrentHistory].duration = 0;
            }

            InputHistory[CurrentHistory].duration++;
            ChargeBuffer();
        }
        
        public bool IsBeingPressed(int index, PlayerInputs input)
        {
            return (InputHistory[index].rawInput & input) != 0;
        }
        public bool WasBeingPressed(int index, PlayerInputs input)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += GlobalVariables.InputHistorySize;

            return (InputHistory[index].rawInput & input) == 0 &&
                (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & input) != 0;
        }
        public bool WasPressed(int index, PlayerInputs input)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += GlobalVariables.InputHistorySize;

            return (InputHistory[index].rawInput & input) != 0 &&
                (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & input) == 0 &&
                InputHistory[index].duration == 1;
        }
        public bool WasReleased(int index, PlayerInputs input)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += GlobalVariables.InputHistorySize;

            return (InputHistory[index].rawInput & input) == 0 &&
                (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & input) != 0 &&
                InputHistory[index].duration == 1;
        }

        public bool IsDifferentInputs(int index)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += GlobalVariables.InputHistorySize;

            return InputHistory[index].rawInput != InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput;
        }

        public bool InputChanged(int index, PlayerInputs input, bool changedThisFrame = true)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += GlobalVariables.InputHistorySize;

            bool currentInputCkech = (InputHistory[index].rawInput & input) != 0;
            bool previousInputCkech = (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & input) != 0;
            bool isRecent = changedThisFrame && InputHistory[index].duration <= 1;
            return currentInputCkech == previousInputCkech && isRecent;
        }

        public bool FaceButtonsChanged(int index)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += GlobalVariables.InputHistorySize;

            bool CurA = (InputHistory[index].rawInput & PlayerInputs.FACE_A) != 0;
            bool CurB = (InputHistory[index].rawInput & PlayerInputs.FACE_B) != 0;
            bool CurC = (InputHistory[index].rawInput & PlayerInputs.FACE_C) != 0;
            bool CurD = (InputHistory[index].rawInput & PlayerInputs.FACE_D) != 0;
            bool CurE = (InputHistory[index].rawInput & PlayerInputs.FACE_E) != 0;
            bool CurF = (InputHistory[index].rawInput & PlayerInputs.FACE_F) != 0;
            bool CurG = (InputHistory[index].rawInput & PlayerInputs.FACE_G) != 0;
            bool CurH = (InputHistory[index].rawInput & PlayerInputs.FACE_H) != 0;

            bool PrevA = (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & PlayerInputs.FACE_A) != 0;
            bool PrevB = (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & PlayerInputs.FACE_B) != 0;
            bool PrevC = (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & PlayerInputs.FACE_C) != 0;
            bool PrevD = (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & PlayerInputs.FACE_D) != 0;
            bool PrevE = (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & PlayerInputs.FACE_E) != 0;
            bool PrevF = (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & PlayerInputs.FACE_F) != 0;
            bool PrevG = (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & PlayerInputs.FACE_G) != 0;
            bool PrevH = (InputHistory[previousInput % GlobalVariables.InputHistorySize].rawInput & PlayerInputs.FACE_H) != 0;

            bool isThisFrame = InputHistory[index].duration <= 1;

            return (CurA != PrevA || CurB != PrevB || CurC != PrevC || CurD != PrevD || 
                    CurE != PrevE || CurF != PrevF || CurG != PrevG || CurH != PrevH) && isThisFrame;
        }

        private void ChargeBuffer()
        {
            if (IsBeingPressed(CurrentHistory, PlayerInputs.LEFT))
            {
                if (hCharge > 0) hCharge = 0;
                hCharge--;
            }
            else if (IsBeingPressed(CurrentHistory, PlayerInputs.RIGHT))
            {
                if (hCharge < 0) hCharge = 0;
                hCharge++;
            }
            else
            {
                if (hCharge != 0) hCharge = 0;
            }

            if (IsBeingPressed(CurrentHistory, PlayerInputs.UP))
            {
                if (vCharge < 0) vCharge = 0;
                vCharge++;
            }
            else if (IsBeingPressed(CurrentHistory, PlayerInputs.DOWN))
            {
                if (vCharge > 0) vCharge = 0;
                vCharge--;
            }
            else
            {
                if (vCharge != 0) vCharge = 0;
            }
        }

        private bool GetButtonState(PlayerInputs buttonFlag, int index, ButtonMode mode)
        {
            switch (mode)
            {
                case ButtonMode.PRESS:
                    return WasPressed(index, buttonFlag);
                case ButtonMode.HOLD:
                    return IsBeingPressed(index, buttonFlag);
                case ButtonMode.RELEASE:
                    return WasReleased(index, buttonFlag);
                case ButtonMode.WAS_PRESSED:
                    return WasBeingPressed(index, buttonFlag);
                case ButtonMode.NOT_PRESSED:
                    return !IsBeingPressed(index, buttonFlag);
                default:
                    return false;
            }
        }

        public InputRegistry CurrentInput() => InputHistory[CurrentHistory];
        public bool IsNeutral() => CurrentInput().IsNull;
    }

    public struct InputRegistry
    {
        public PlayerInputs rawInput;
        public ushort duration;

        public bool IsNull => rawInput == 0;
        
        public override string ToString()
        {
            return $"({rawInput}, {duration})";
        }

    };
}
