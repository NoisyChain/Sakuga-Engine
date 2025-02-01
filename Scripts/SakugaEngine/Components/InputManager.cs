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

                    int directionNumber = (int)motion.ValidInputs[i].Inputs[j].Directional;
                    int buttonNumber = (int)motion.ValidInputs[i].Inputs[j].Buttons;

                    bool validBuffer = motion.InputBuffer == 0 ||
                        InputHistory[HistoryIndex].duration <= motion.InputBuffer;

                    bool validInput;

                    if (directionNumber != 5 && buttonNumber == 0)
                        validInput = CheckDirectionalInputs(HistoryIndex, directionNumber, (int)motion.ValidInputs[i].Inputs[j].DirectionalMode, motion.AbsoluteDirection);
                    else if (directionNumber == 5 && buttonNumber > 0)
                        validInput = CheckButtonInputs(HistoryIndex, buttonNumber, (int)motion.ValidInputs[i].Inputs[j].ButtonMode);
                    else
                        validInput = CheckDirectionalInputs(HistoryIndex, directionNumber, (int)motion.ValidInputs[i].Inputs[j].DirectionalMode, motion.AbsoluteDirection) &&
                        CheckButtonInputs(HistoryIndex, buttonNumber, (int)motion.ValidInputs[i].Inputs[j].ButtonMode);

                    bool chargedInput = CheckChargeInputs(HistoryIndex, directionNumber, motion.DirectionalChargeLimit);
                    
                    inputFound = (validBuffer && validInput) || chargedInput;
                    if (!inputFound) break;
                }
                if (inputFound) break;
            }
            
            return inputFound;
        }

        public bool CheckInputEnd(MotionInputs motion)
        {
            int directionNumber = (int)motion.ValidInputs[0].Inputs[motion.ValidInputs[0].Inputs.Length - 1].Directional;
            int buttonNumber = (int)motion.ValidInputs[0].Inputs[motion.ValidInputs[0].Inputs.Length - 1].Buttons;

            bool validInput;

            if (directionNumber != 5 && buttonNumber == 0)
                validInput = !CheckDirectionalInputs(CurrentHistory, directionNumber, 1, motion.AbsoluteDirection);
            else if (directionNumber == 5 && buttonNumber > 0)
                validInput = !CheckButtonInputs(CurrentHistory, buttonNumber, 1);
            else
                validInput = !CheckDirectionalInputs(CurrentHistory, directionNumber, 1, motion.AbsoluteDirection) &&
                !CheckButtonInputs(CurrentHistory, buttonNumber, 1);

            if (!validInput)
                return false;
            
            return true;
        }

        public bool CheckDirectionalInputs(int index, int buttonNumber, int buttonMode, bool absDirection)
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
                case 0:
                    left = WasPressed(index, _left);
                    right = WasPressed(index, _right);
                    up = WasPressed(index, Global.INPUT_UP);
                    down = WasPressed(index, Global.INPUT_DOWN);
                    break;
                case 1:
                    left = IsBeingPressed(index, _left);
                    right = IsBeingPressed(index, _right);
                    up = IsBeingPressed(index, Global.INPUT_UP);
                    down = IsBeingPressed(index, Global.INPUT_DOWN);
                    break;
                case 2:
                    left = WasReleased(index, _left);
                    right = WasReleased(index, _right);
                    up = WasReleased(index, Global.INPUT_UP);
                    down = WasReleased(index, Global.INPUT_DOWN);
                    break;
                case 3:
                    left = WasBeingPressed(index, _left);
                    right = WasBeingPressed(index, _right);
                    up = WasBeingPressed(index, Global.INPUT_UP);
                    down = WasBeingPressed(index, Global.INPUT_DOWN);
                    break;
                case 4:
                    left = !IsBeingPressed(index, _left);
                    right = !IsBeingPressed(index, _right);
                    up = !IsBeingPressed(index, Global.INPUT_UP);
                    down = !IsBeingPressed(index, Global.INPUT_DOWN);
                    break;
            }
            
            bool absV = absDirection ? !up && !down : true;
            bool absH = absDirection ? !left && !right : true;

            if (buttonMode == 4) return (buttonNumber == 2 && down) ||
                (buttonNumber == 4 && left) ||
                (buttonNumber == 6 && right) ||
                (buttonNumber == 8 && up) ||
                (buttonNumber == 1 && down && left) ||
                (buttonNumber == 3 && down && right) ||
                (buttonNumber == 7 && up && left) ||
                (buttonNumber == 9 && up && right) ||
                (buttonNumber == 5 && down && up && left && right);

            else return (buttonNumber == 2 && down && !up && absH) ||
                (buttonNumber == 4 && absV && left && !right) ||
                (buttonNumber == 6 && absV && !left && right) ||
                (buttonNumber == 8 && !down && up && absH) ||
                (buttonNumber == 1 && down && !up && left && !right) ||
                (buttonNumber == 3 && down && !up && !left && right) ||
                (buttonNumber == 7 && !down && up && left && !right) ||
                (buttonNumber == 9 && !down && up && !left && right) ||
                (buttonNumber == 5 && !down && !up && !left && !right);
        }

        public bool CheckButtonInputs(int index, int buttonNumber, int buttonMode)
        {
            bool action_b1 = false;
            bool action_b2 = false;
            bool action_b3 = false;
            bool action_b4 = false;

            switch (buttonMode)
            {
                case 0:
                    action_b1 = WasPressed(index, Global.INPUT_FACE_A);
                    action_b2 = WasPressed(index, Global.INPUT_FACE_B);
                    action_b3 = WasPressed(index, Global.INPUT_FACE_C);
                    action_b4 = WasPressed(index, Global.INPUT_FACE_D);
                    break;
                case 1:
                    action_b1 = IsBeingPressed(index, Global.INPUT_FACE_A);
                    action_b2 = IsBeingPressed(index, Global.INPUT_FACE_B);
                    action_b3 = IsBeingPressed(index, Global.INPUT_FACE_C);
                    action_b4 = IsBeingPressed(index, Global.INPUT_FACE_D);
                    break;
                case 2:
                    action_b1 = WasReleased(index, Global.INPUT_FACE_A);
                    action_b2 = WasReleased(index, Global.INPUT_FACE_B);
                    action_b3 = WasReleased(index, Global.INPUT_FACE_C);
                    action_b4 = WasReleased(index, Global.INPUT_FACE_D);
                    break;
                case 3:
                    action_b1 = WasBeingPressed(index, Global.INPUT_FACE_A);
                    action_b2 = WasBeingPressed(index, Global.INPUT_FACE_B);
                    action_b3 = WasBeingPressed(index, Global.INPUT_FACE_C);
                    action_b4 = WasBeingPressed(index, Global.INPUT_FACE_D);
                    break;
                case 4:
                    action_b1 = !IsBeingPressed(index, Global.INPUT_FACE_A);
                    action_b2 = !IsBeingPressed(index, Global.INPUT_FACE_B);
                    action_b3 = !IsBeingPressed(index, Global.INPUT_FACE_C);
                    action_b4 = !IsBeingPressed(index, Global.INPUT_FACE_D);
                    break;
            }

            
            return (buttonNumber == 0 && !action_b1 && !action_b2 && !action_b3 && !action_b4) ||
                (buttonNumber == 1 && action_b1) ||
                (buttonNumber == 2 && action_b2) ||
                (buttonNumber == 3 && action_b3) ||
                (buttonNumber == 4 && action_b4) ||
                (buttonNumber == 5 && action_b1 && action_b2) ||
                (buttonNumber == 6 && action_b1 && action_b3) ||
                (buttonNumber == 7 && action_b2 && action_b3) ||
                (buttonNumber == 8 && action_b1 && action_b2 && action_b3) ||
                (buttonNumber == 9 && action_b1 && action_b2 && action_b3 && action_b4) ||
                (buttonNumber == 10 && (action_b1 || action_b2 || action_b3 || action_b4));
        }

        public bool CheckChargeInputs(int index, int buttonNumber, int DirectionalChargeLimit)
        {
            bool _left = IsBeingPressed(index, Global.INPUT_LEFT);
            bool _right = IsBeingPressed(index, Global.INPUT_RIGHT);
            
            bool left = _left;
            if (InputSide < 0) left = _right;

            bool right = _right;
            if (InputSide < 0) right = _left;
            
            bool up = IsBeingPressed(index, Global.INPUT_UP);
            bool down = IsBeingPressed(index, Global.INPUT_DOWN);

            bool checkInputs = (buttonNumber == 10 && left && Mathf.Abs(InputHistory[index].hCharge) >= DirectionalChargeLimit) ||
                (buttonNumber == 11 && down && Mathf.Abs(InputHistory[index].vCharge) >= DirectionalChargeLimit) ||
                (buttonNumber == 12 && left && Mathf.Abs(InputHistory[index].hCharge) >= DirectionalChargeLimit && down && InputHistory[index].vCharge <= -DirectionalChargeLimit) ||
                (buttonNumber == 13 && left && Mathf.Abs(InputHistory[index].hCharge) >= DirectionalChargeLimit && up && InputHistory[index].vCharge >= DirectionalChargeLimit);
            //bool isChargedEnough = InputHistory[index].duration >= DirectionalChargeLimit;

            return checkInputs;// && isChargedEnough;
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
                if (InputHistory[CurrentHistory].bCharge > 0 && FaceButtonsChanged(CurrentHistory))
                    InputHistory[CurrentHistory].bCharge = 0;
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
