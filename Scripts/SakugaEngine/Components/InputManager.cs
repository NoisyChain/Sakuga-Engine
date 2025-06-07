using Godot;
using System.IO;
using SakugaEngine.Resources;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class InputManager : Node
    {
        public InputRegistry[] InputHistory = new InputRegistry[Global.InputHistorySize];
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
            bool left = false;
            bool right = false;
            bool up = false;
            bool down = false;

            int _left = Global.INPUT_LEFT;
            if (InputSide < 0) _left = Global.INPUT_RIGHT;

            int _right = Global.INPUT_RIGHT;
            if (InputSide < 0) _right = Global.INPUT_LEFT;

            switch (buttonMode)
            {
                case Global.ButtonMode.PRESS:
                    left = WasPressed(index, _left);
                    right = WasPressed(index, _right);
                    up = WasPressed(index, Global.INPUT_UP);
                    down = WasPressed(index, Global.INPUT_DOWN);
                    break;
                case Global.ButtonMode.HOLD:
                    left = IsBeingPressed(index, _left);
                    right = IsBeingPressed(index, _right);
                    up = IsBeingPressed(index, Global.INPUT_UP);
                    down = IsBeingPressed(index, Global.INPUT_DOWN);
                    break;
                case Global.ButtonMode.RELEASE:
                    left = WasReleased(index, _left);
                    right = WasReleased(index, _right);
                    up = WasReleased(index, Global.INPUT_UP);
                    down = WasReleased(index, Global.INPUT_DOWN);
                    break;
                case Global.ButtonMode.WAS_PRESSED:
                    left = WasBeingPressed(index, _left);
                    right = WasBeingPressed(index, _right);
                    up = WasBeingPressed(index, Global.INPUT_UP);
                    down = WasBeingPressed(index, Global.INPUT_DOWN);
                    break;
                case Global.ButtonMode.NOT_PRESSED:
                    left = !IsBeingPressed(index, _left);
                    right = !IsBeingPressed(index, _right);
                    up = !IsBeingPressed(index, Global.INPUT_UP);
                    down = !IsBeingPressed(index, Global.INPUT_DOWN);
                    break;
            }

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
            bool action_ba = false;
            bool action_bb = false;
            bool action_bc = false;
            bool action_bd = false;

            switch (buttonMode)
            {
                case Global.ButtonMode.PRESS:
                    action_ba = WasPressed(index, Global.INPUT_FACE_A);
                    action_bb = WasPressed(index, Global.INPUT_FACE_B);
                    action_bc = WasPressed(index, Global.INPUT_FACE_C);
                    action_bd = WasPressed(index, Global.INPUT_FACE_D);
                    break;
                case Global.ButtonMode.HOLD:
                    action_ba = IsBeingPressed(index, Global.INPUT_FACE_A);
                    action_bb = IsBeingPressed(index, Global.INPUT_FACE_B);
                    action_bc = IsBeingPressed(index, Global.INPUT_FACE_C);
                    action_bd = IsBeingPressed(index, Global.INPUT_FACE_D);
                    break;
                case Global.ButtonMode.RELEASE:
                    action_ba = WasReleased(index, Global.INPUT_FACE_A);
                    action_bb = WasReleased(index, Global.INPUT_FACE_B);
                    action_bc = WasReleased(index, Global.INPUT_FACE_C);
                    action_bd = WasReleased(index, Global.INPUT_FACE_D);
                    break;
                case Global.ButtonMode.WAS_PRESSED:
                    action_ba = WasBeingPressed(index, Global.INPUT_FACE_A);
                    action_bb = WasBeingPressed(index, Global.INPUT_FACE_B);
                    action_bc = WasBeingPressed(index, Global.INPUT_FACE_C);
                    action_bd = WasBeingPressed(index, Global.INPUT_FACE_D);
                    break;
                case Global.ButtonMode.NOT_PRESSED:
                    action_ba = !IsBeingPressed(index, Global.INPUT_FACE_A);
                    action_bb = !IsBeingPressed(index, Global.INPUT_FACE_B);
                    action_bc = !IsBeingPressed(index, Global.INPUT_FACE_C);
                    action_bd = !IsBeingPressed(index, Global.INPUT_FACE_D);
                    break;
            }

            bool canA = (buttonNumber & Global.ButtonInputs.FACE_A) > 0;
            bool canB = (buttonNumber & Global.ButtonInputs.FACE_B) > 0;
            bool canC = (buttonNumber & Global.ButtonInputs.FACE_C) > 0;
            bool canD = (buttonNumber & Global.ButtonInputs.FACE_D) > 0;

            return (buttonNumber == 0 && !action_ba && !action_bb && !action_bc && !action_bd) ||
                (!canA || canA == action_ba) && (!canB || canB == action_bb) &&
                (!canC || canC == action_bc) && (!canD || canD == action_bd);
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

            bool checkInputs = (buttonNumber == Global.DirectionalInputs.LEFT && left && Mathf.Abs(InputHistory[index].hCharge) >= dirCharge) ||
                (buttonNumber == Global.DirectionalInputs.DOWN && down && Mathf.Abs(InputHistory[index].vCharge) >= dirCharge) ||
                (buttonNumber == Global.DirectionalInputs.DOWN_LEFT && left && Mathf.Abs(InputHistory[index].hCharge) >= dirCharge && down && InputHistory[index].vCharge <= -dirCharge) ||
                (buttonNumber == Global.DirectionalInputs.UP_LEFT && left && Mathf.Abs(InputHistory[index].hCharge) >= dirCharge && up && InputHistory[index].vCharge >= dirCharge);

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
                int previousInput = CurrentHistory - 1;
                if (previousInput < 0) previousInput += Global.InputHistorySize;

                InputHistory[CurrentHistory].hCharge = InputHistory[previousInput].hCharge;
                InputHistory[CurrentHistory].vCharge = InputHistory[previousInput].vCharge;
                InputHistory[CurrentHistory].bCharge = InputHistory[previousInput].bCharge;
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
                if (InputHistory[CurrentHistory].hCharge > 0) InputHistory[CurrentHistory].hCharge = 0;
                InputHistory[CurrentHistory].hCharge--;
            }
            else if (IsBeingPressed(CurrentHistory, Global.INPUT_RIGHT))
            {
                if (InputHistory[CurrentHistory].hCharge < 0) InputHistory[CurrentHistory].hCharge = 0;
                InputHistory[CurrentHistory].hCharge++;
            }
            else
            {
                if (InputHistory[CurrentHistory].hCharge != 0) InputHistory[CurrentHistory].hCharge = 0;
            }

            if (IsBeingPressed(CurrentHistory, Global.INPUT_UP))
            {
                if (InputHistory[CurrentHistory].vCharge < 0) InputHistory[CurrentHistory].vCharge = 0;
                InputHistory[CurrentHistory].vCharge++;
            }
            else if (IsBeingPressed(CurrentHistory, Global.INPUT_DOWN))
            {
                if (InputHistory[CurrentHistory].vCharge > 0) InputHistory[CurrentHistory].vCharge = 0;
                InputHistory[CurrentHistory].vCharge--;
            }
            else
            {
                if (InputHistory[CurrentHistory].vCharge != 0) InputHistory[CurrentHistory].vCharge = 0;
            }

            if (IsBeingPressed(CurrentHistory, Global.INPUT_ANY_BUTTON))
            {
                InputHistory[CurrentHistory].bCharge++;
            }
            else
            {
                InputHistory[CurrentHistory].bCharge = 0;
            }
        }

        public InputRegistry CurrentInput() => InputHistory[CurrentHistory];
        public ushort InputBufferDuration() => CurrentInput().bCharge;
        public bool IsNeutral() => CurrentInput().IsNull;

        public void Serialize(BinaryWriter bw)
        {
            for (int i = 0; i < Global.InputHistorySize; i++)
                InputHistory[i].Serialize(bw);
            
            bw.Write(CurrentHistory);
            bw.Write(InputSide);
        }

        public void Deserialize(BinaryReader br)
        {
            for (int i = 0; i < Global.InputHistorySize; i++)
                InputHistory[i].Deserialize(br);
            
            CurrentHistory = br.ReadInt32();
            InputSide = br.ReadInt32();
        }

        
    }

    [System.Serializable]
    public struct InputRegistry
    {
        public ushort rawInput;
        public ushort duration;
        public short hCharge;
        public short vCharge;
        public ushort bCharge;

        public bool IsNull => rawInput == 0;

        public void Serialize(BinaryWriter bw)
        {
            bw.Write(rawInput);
            bw.Write(duration);
            bw.Write(hCharge);
            bw.Write(vCharge);
            bw.Write(bCharge);
        }

        public void Deserialize(BinaryReader br)
        {
            rawInput = br.ReadUInt16();
            duration = br.ReadUInt16();
            hCharge = br.ReadInt16();
            vCharge = br.ReadInt16();
            bCharge = br.ReadUInt16();
        }
        
        public override string ToString()
        {
            return $"({rawInput}, {duration})";
        }

    };
}
