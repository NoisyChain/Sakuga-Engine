using Godot;
using System.IO;
using SakugaEngine.Resources;

namespace SakugaEngine
{
    [GlobalClass]
    public partial class InputManager : Node
    {
        //public FighterButton h;
        //public FighterButton v;
        //public FighterButton b1;
        //public FighterButton b2;
        //public FighterButton b3;
        //public FighterButton b4;
        public InputRegistry[] InputHistory = new InputRegistry[Global.InputHistorySize];
        public int CurrentHistory = 0;
        public int InputSide;

        public bool IsNeutral() => InputHistory[CurrentHistory].IsNull;

        public bool CheckMotionInputs(MotionInputs motion)
        {
            //Define the first input to check
            //If the result is less than 0, cycle it to the end
            int startingInput = (CurrentHistory - motion.Inputs.Length) + 1;
            if (startingInput < 0) startingInput += Global.InputHistorySize;

            for (int i = 0; i < motion.Inputs.Length; i++)
            {
                int HistoryIndex = (startingInput + i) % Global.InputHistorySize;

                int directionNumber = (int)motion.Inputs[i].Directional;
                int buttonNumber = (int)motion.Inputs[i].Buttons;

                bool validBuffer = motion.InputBuffer == 0 ||
                    InputHistory[HistoryIndex].duration <= motion.InputBuffer;

                bool validInput;

                if (directionNumber != 5 && buttonNumber == 0)
                    validInput = CheckDirectionalInputs(HistoryIndex, directionNumber, (int)motion.Inputs[i].DirectionalMode, motion.AbsoluteDirection);
                else if (directionNumber == 5 && buttonNumber > 0)
                    validInput = CheckButtonInputs(HistoryIndex, buttonNumber, (int)motion.Inputs[i].ButtonMode);
                else
                    validInput = CheckDirectionalInputs(HistoryIndex, directionNumber, (int)motion.Inputs[i].DirectionalMode, motion.AbsoluteDirection) &&
                    CheckButtonInputs(HistoryIndex, buttonNumber, (int)motion.Inputs[i].ButtonMode);

                if ((!validBuffer || !validInput) && !CheckChargeInputs(HistoryIndex, directionNumber, motion.DirectionalChargeLimit)) 
                    return false;
            }
            
            return true;
        }

        public bool CheckInputEnd(MotionInputs motion)
        {
            int directionNumber = (int)motion.Inputs[motion.Inputs.Length - 1].Directional;
            int buttonNumber = (int)motion.Inputs[motion.Inputs.Length - 1].Buttons;

            bool validInput;

            if (directionNumber != 5 && buttonNumber == 0)
                validInput = CheckDirectionalInputs(CurrentHistory, directionNumber, 3, motion.AbsoluteDirection);
            else if (directionNumber == 5 && buttonNumber > 0)
                validInput = CheckButtonInputs(CurrentHistory, buttonNumber, 3);
            else
                validInput = CheckDirectionalInputs(CurrentHistory, directionNumber, 3, motion.AbsoluteDirection) &&
                CheckButtonInputs(CurrentHistory, buttonNumber, 3);

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
            }
            
            bool absV = absDirection ? !up && !down : true;
            bool absH = absDirection ? !left && !right : true;

            return (buttonNumber == 2 && down && !up && absH) ||
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

            bool checkInputs = (buttonNumber == 10 && left) ||
                (buttonNumber == 11 && down) ||
                (buttonNumber == 12 && left && down) ||
                (buttonNumber == 13 && left && up);
            bool isChargedEnough = InputHistory[index].duration >= DirectionalChargeLimit;

            return checkInputs && isChargedEnough;
        }

        public void InsertToHistory(ushort input)
        {
            if (InputHistory[CurrentHistory].rawInput != input)
            {
                CurrentHistory++;
                if (CurrentHistory >= Global.InputHistorySize) CurrentHistory = 0;

                InputHistory[CurrentHistory].rawInput = input;
                InputHistory[CurrentHistory].duration = 0;
            }

            InputHistory[CurrentHistory].duration++;
            
            //GD.Print(CurrentHistory + "::" + InputHistory[CurrentHistory].ToString());
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
}

[System.Serializable]
public struct InputRegistry
{
    public ushort rawInput;
    public ushort duration;

    public bool IsNull => rawInput == 0;

    public void Serialize(BinaryWriter bw)
    {
        bw.Write(rawInput);
        bw.Write(duration);
    }

    public void Deserialize(BinaryReader br)
    {
        rawInput = br.ReadUInt16();
        duration = br.ReadUInt16();
    }
    
    public override string ToString()
    {
        return $"({rawInput}, {duration})";
    }

};
