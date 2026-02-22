using Godot;
using System.IO;
using SakugaEngine.Resources;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class InputManager : Node
    {
        public InputRegistry[] InputHistory = new InputRegistry[Global.InputHistorySize];
        public short hCharge;
        public short vCharge;
        public int CurrentHistory = 0;
        public int InputSide;

        public bool CheckMotionInputs(MotionInputs motion)
        {
            if (motion == null) return false;
            if (motion.ValidInputs == null) return false;
            
            bool inputFound = false;

            for (int i = 0; i < motion.ValidInputs.Length; i++)
            {
                //Define the first input to check
                //If the result is less than 0, cycle it to the end
                int startingInput = (CurrentHistory - motion.ValidInputs[i].Inputs.Length) + 1;
                if (startingInput < 0) startingInput += Global.InputHistorySize;

                for (int j = 0; j < motion.ValidInputs[i].Inputs.Length; j++)
                {
                    int HistoryIndex = (startingInput + j) % Global.InputHistorySize;

                    Global.DirectionalInputs directionals = motion.ValidInputs[i].Inputs[j].Directional;
                    Global.ButtonInputs buttons = motion.ValidInputs[i].Inputs[j].Buttons;

                    Global.ButtonMode dirMode = motion.ValidInputs[i].Inputs[j].DirectionalMode;
                    Global.ButtonMode butMode = motion.ValidInputs[i].Inputs[j].ButtonMode;

                    bool validBuffer = motion.InputBuffer == 0 ||
                        InputHistory[HistoryIndex].duration <= motion.InputBuffer;

                    bool validInput;

                    if (directionals > 0 && buttons == 0)
                        validInput = CheckDirectionalInputs(HistoryIndex, directionals, dirMode, motion.AbsoluteDirection);
                    else if (directionals == 0 && buttons > 0)
                        validInput = CheckButtonInputs(HistoryIndex, buttons, butMode);
                    else
                        validInput = CheckDirectionalInputs(HistoryIndex, directionals, dirMode, motion.AbsoluteDirection) &&
                        CheckButtonInputs(HistoryIndex, buttons, butMode);
                        
                    bool chargedInput = CheckChargeInputs(HistoryIndex, directionals, motion.DirectionalChargeLimit);
                    
                    inputFound = (validBuffer && validInput) || chargedInput;
                    if (!inputFound) break;
                }
                if (inputFound) break;
            }
            
            return inputFound;
        }

        public bool CheckInputEnd(MotionInputs motion)
        {
            Global.DirectionalInputs directionals = motion.ValidInputs[0].Inputs[motion.ValidInputs[0].Inputs.Length - 1].Directional;
            Global.ButtonInputs buttons = motion.ValidInputs[0].Inputs[motion.ValidInputs[0].Inputs.Length - 1].Buttons;

            bool validInput;

            if (directionals > 0 && buttons == 0)
                validInput = !CheckDirectionalInputs(CurrentHistory, directionals, Global.ButtonMode.HOLD, motion.AbsoluteDirection);
            else if (directionals == 0 && buttons > 0)
                validInput = !CheckButtonInputs(CurrentHistory, buttons, Global.ButtonMode.HOLD);
            else
                validInput = !CheckDirectionalInputs(CurrentHistory, directionals, Global.ButtonMode.HOLD, motion.AbsoluteDirection) &&
                !CheckButtonInputs(CurrentHistory, buttons, Global.ButtonMode.HOLD);

            if (!validInput)
                return false;
            
            return true;
        }

        public bool CheckDirectionalInputs(int index, Global.DirectionalInputs buttonNumber, Global.ButtonMode buttonMode, bool absDirection)
        {
            int _left = Global.INPUT_LEFT;
            if (InputSide < 0) _left = Global.INPUT_RIGHT;

            int _right = Global.INPUT_RIGHT;
            if (InputSide < 0) _right = Global.INPUT_LEFT;

            bool left = GetButtonState(_left, index, buttonMode);
            bool right = GetButtonState(_right, index, buttonMode);
            bool up = GetButtonState(Global.INPUT_UP, index, buttonMode);
            bool down = GetButtonState(Global.INPUT_DOWN, index, buttonMode);

            bool absV = absDirection ? !up && !down : true;
            bool absH = absDirection ? !left && !right : true;

            bool notP = buttonMode == Global.ButtonMode.NOT_PRESSED;
            bool canAbsH = notP || absH;
            bool canAbsV = notP || absV;
            bool neutralDirection = notP ? (down && up && left && right) : (!down && !up && !left && !right);
            
            return (buttonNumber == Global.DirectionalInputs.DOWN && down && canAbsH) ||
                (buttonNumber == Global.DirectionalInputs.LEFT && left && canAbsV) ||
                (buttonNumber == Global.DirectionalInputs.RIGHT && right && canAbsV) ||
                (buttonNumber == Global.DirectionalInputs.UP && up && canAbsH) ||
                (buttonNumber == Global.DirectionalInputs.DOWN_LEFT && down && left) ||
                (buttonNumber == Global.DirectionalInputs.DOWN_RIGHT && down && right) ||
                (buttonNumber == Global.DirectionalInputs.UP_LEFT && up && left) ||
                (buttonNumber == Global.DirectionalInputs.UP_RIGHT && up && right) ||
                (buttonNumber == 0 && neutralDirection);
        }

        public bool CheckButtonInputs(int index, Global.ButtonInputs buttonNumber, Global.ButtonMode buttonMode)
        {
            bool action_ba = GetButtonState(Global.INPUT_FACE_A, index, buttonMode);
            bool action_bb = GetButtonState(Global.INPUT_FACE_B, index, buttonMode);
            bool action_bc = GetButtonState(Global.INPUT_FACE_C, index, buttonMode);
            bool action_bd = GetButtonState(Global.INPUT_FACE_D, index, buttonMode);
            bool action_be = GetButtonState(Global.INPUT_FACE_E, index, buttonMode);
            bool action_bf = GetButtonState(Global.INPUT_FACE_F, index, buttonMode);
            bool action_bg = GetButtonState(Global.INPUT_FACE_G, index, buttonMode);
            bool action_bh = GetButtonState(Global.INPUT_FACE_H, index, buttonMode);

            bool canA = (buttonNumber & Global.ButtonInputs.FACE_A) > 0;
            bool canB = (buttonNumber & Global.ButtonInputs.FACE_B) > 0;
            bool canC = (buttonNumber & Global.ButtonInputs.FACE_C) > 0;
            bool canD = (buttonNumber & Global.ButtonInputs.FACE_D) > 0;
            bool canE = (buttonNumber & Global.ButtonInputs.FACE_E) > 0;
            bool canF = (buttonNumber & Global.ButtonInputs.FACE_F) > 0;
            bool canG = (buttonNumber & Global.ButtonInputs.FACE_G) > 0;
            bool canH = (buttonNumber & Global.ButtonInputs.FACE_H) > 0;

            return (buttonNumber == 0 && !action_ba && !action_bb && !action_bc && !action_bd && !action_be && !action_bf && !action_bg && !action_bh) ||
                (!canA || canA == action_ba) && (!canB || canB == action_bb) && (!canC || canC == action_bc) && (!canD || canD == action_bd) && 
                (!canE || canE == action_be) && (!canF || canF == action_bf) && (!canG || canG == action_bg) && (!canH || canH == action_bh);
        }

        public bool CheckChargeInputs(int index, Global.DirectionalInputs buttonNumber, int dirCharge)
        {
            if (dirCharge == 0) return false;

            bool _left = IsBeingPressed(index, Global.INPUT_LEFT);
            bool _right = IsBeingPressed(index, Global.INPUT_RIGHT);
            
            bool left = _left;
            if (InputSide < 0) left = _right;

            bool right = _right;
            if (InputSide < 0) right = _left;
            
            bool up = IsBeingPressed(index, Global.INPUT_UP);
            bool down = IsBeingPressed(index, Global.INPUT_DOWN);

            bool checkInputs = (buttonNumber == Global.DirectionalInputs.LEFT && left && Mathf.Abs(hCharge) >= dirCharge) ||
                (buttonNumber == Global.DirectionalInputs.DOWN && down && Mathf.Abs(vCharge) >= dirCharge) ||
                (buttonNumber == Global.DirectionalInputs.DOWN_LEFT && left && Mathf.Abs(hCharge) >= dirCharge && down && vCharge <= -dirCharge) ||
                (buttonNumber == Global.DirectionalInputs.UP_LEFT && left && Mathf.Abs(hCharge) >= dirCharge && up && vCharge >= dirCharge);

            return checkInputs;
        }

        public void InsertToHistory(ushort input)
        {
            if (InputHistory[CurrentHistory].rawInput != input)
            {
                CurrentHistory++;
                if (CurrentHistory >= Global.InputHistorySize) CurrentHistory = 0;

                InputHistory[CurrentHistory].rawInput = input;
                InputHistory[CurrentHistory].duration = 0;

                //Get the charge values from the previous input
                //int previousInput = CurrentHistory - 1;
                //if (previousInput < 0) previousInput += Global.InputHistorySize;

                //InputHistory[CurrentHistory].hCharge = InputHistory[previousInput].hCharge;
                //InputHistory[CurrentHistory].vCharge = InputHistory[previousInput].vCharge;
                //InputHistory[CurrentHistory].bCharge = InputHistory[previousInput].bCharge;
            }

            InputHistory[CurrentHistory].duration++;
            ChargeBuffer();
        }
        
        public bool IsBeingPressed(int index, int input)
        {
            return (InputHistory[index].rawInput & input) != 0;
        }
        public bool WasBeingPressed(int index, int input)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += Global.InputHistorySize;

            return (InputHistory[index].rawInput & input) == 0 &&
                (InputHistory[previousInput % Global.InputHistorySize].rawInput & input) != 0;
        }
        public bool WasPressed(int index, int input)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += Global.InputHistorySize;

            return (InputHistory[index].rawInput & input) != 0 &&
                (InputHistory[previousInput % Global.InputHistorySize].rawInput & input) == 0 &&
                InputHistory[index].duration == 1;
        }
        public bool WasReleased(int index, int input)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += Global.InputHistorySize;

            return (InputHistory[index].rawInput & input) == 0 &&
                (InputHistory[previousInput % Global.InputHistorySize].rawInput & input) != 0 &&
                InputHistory[index].duration == 1;
        }

        public bool IsDifferentInputs(int index)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += Global.InputHistorySize;

            return InputHistory[index].rawInput != InputHistory[previousInput % Global.InputHistorySize].rawInput;
        }

        public bool InputChanged(int index, int input, bool changedThisFrame = true)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += Global.InputHistorySize;

            bool currentInputCkech = (InputHistory[index].rawInput & input) != 0;
            bool previousInputCkech = (InputHistory[previousInput % Global.InputHistorySize].rawInput & input) != 0;
            bool isRecent = changedThisFrame && InputHistory[index].duration <= 1;
            return currentInputCkech == previousInputCkech && isRecent;
        }

        public bool FaceButtonsChanged(int index)
        {
            int previousInput = index - 1;
            if (previousInput < 0) previousInput += Global.InputHistorySize;

            bool CurA = (InputHistory[index].rawInput & Global.INPUT_FACE_A) != 0;
            bool CurB = (InputHistory[index].rawInput & Global.INPUT_FACE_B) != 0;
            bool CurC = (InputHistory[index].rawInput & Global.INPUT_FACE_C) != 0;
            bool CurD = (InputHistory[index].rawInput & Global.INPUT_FACE_D) != 0;

            bool PrevA = (InputHistory[previousInput % Global.InputHistorySize].rawInput & Global.INPUT_FACE_A) != 0;
            bool PrevB = (InputHistory[previousInput % Global.InputHistorySize].rawInput & Global.INPUT_FACE_B) != 0;
            bool PrevC = (InputHistory[previousInput % Global.InputHistorySize].rawInput & Global.INPUT_FACE_C) != 0;
            bool PrevD = (InputHistory[previousInput % Global.InputHistorySize].rawInput & Global.INPUT_FACE_D) != 0;

            bool isThisFrame = InputHistory[index].duration <= 1;

            return (CurA != PrevA || CurB != PrevB || CurC != PrevC || CurD != PrevD) && isThisFrame;
        }

        private void ChargeBuffer()
        {
            if (IsBeingPressed(CurrentHistory, Global.INPUT_LEFT))
            {
                if (hCharge > 0) hCharge = 0;
                hCharge--;
            }
            else if (IsBeingPressed(CurrentHistory, Global.INPUT_RIGHT))
            {
                if (hCharge < 0) hCharge = 0;
                hCharge++;
            }
            else
            {
                if (hCharge != 0) hCharge = 0;
            }

            if (IsBeingPressed(CurrentHistory, Global.INPUT_UP))
            {
                if (vCharge < 0) vCharge = 0;
                vCharge++;
            }
            else if (IsBeingPressed(CurrentHistory, Global.INPUT_DOWN))
            {
                if (vCharge > 0) vCharge = 0;
                vCharge--;
            }
            else
            {
                if (vCharge != 0) vCharge = 0;
            }
        }

        private bool GetButtonState(int buttonFlag, int index, Global.ButtonMode mode)
        {
            switch (mode)
            {
                case Global.ButtonMode.PRESS:
                    return WasPressed(index, buttonFlag);
                case Global.ButtonMode.HOLD:
                    return IsBeingPressed(index, buttonFlag);
                case Global.ButtonMode.RELEASE:
                    return WasReleased(index, buttonFlag);
                case Global.ButtonMode.WAS_PRESSED:
                    return WasBeingPressed(index, buttonFlag);
                case Global.ButtonMode.NOT_PRESSED:
                    return !IsBeingPressed(index, buttonFlag);
                default:
                    return false;
            }
        }

        public InputRegistry CurrentInput() => InputHistory[CurrentHistory];
        //public ushort InputBufferDuration() => CurrentInput().bCharge;
        public bool IsNeutral() => CurrentInput().IsNull;

        public void Serialize(BinaryWriter bw)
        {
            for (int i = 0; i < Global.InputHistorySize; i++)
                InputHistory[i].Serialize(bw);
            
            bw.Write(CurrentHistory);
            bw.Write(InputSide);
            bw.Write(hCharge);
            bw.Write(vCharge);
        }

        public void Deserialize(BinaryReader br)
        {
            for (int i = 0; i < Global.InputHistorySize; i++)
                InputHistory[i].Deserialize(br);
            
            CurrentHistory = br.ReadInt32();
            InputSide = br.ReadInt32();
            hCharge = br.ReadInt16();
            vCharge = br.ReadInt16();
        }

        
    }

    [System.Serializable]
    public struct InputRegistry
    {
        public ushort rawInput;
        public ushort duration;
        //public short hCharge;
        //public short vCharge;
        //public ushort bCharge;

        public bool IsNull => rawInput == 0;

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(rawInput);
            bw.Write(duration);
            //bw.Write(hCharge);
            //bw.Write(vCharge);
            //bw.Write(bCharge);
        }

        public void Deserialize(BinaryReader br)
        {
            rawInput = br.ReadUInt16();
            duration = br.ReadUInt16();
            //hCharge = br.ReadInt16();
            //vCharge = br.ReadInt16();
            //bCharge = br.ReadUInt16();
        }
        
        public override string ToString()
        {
            return $"({rawInput}, {duration})";
        }

    };
}
