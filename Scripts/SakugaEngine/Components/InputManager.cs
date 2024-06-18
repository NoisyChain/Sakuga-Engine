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
        private int CurrentHistory = 0;
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
            bool _left = false;
            bool _right = false;
            bool up = false;
            bool down = false;

            switch (buttonMode)
            {
                case 0:
                    _left = WasPressed(index, Global.INPUT_LEFT);
                    _right = WasPressed(index, Global.INPUT_RIGHT);
                    up = WasPressed(index, Global.INPUT_UP);
                    down = WasPressed(index, Global.INPUT_DOWN);
                    break;
                case 1:
                    _left = IsBeingPressed(index, Global.INPUT_LEFT);
                    _right = IsBeingPressed(index, Global.INPUT_RIGHT);
                    up = IsBeingPressed(index, Global.INPUT_UP);
                    down = IsBeingPressed(index, Global.INPUT_DOWN);
                    break;
                case 2:
                    _left = WasReleased(index, Global.INPUT_LEFT);
                    _right = WasReleased(index, Global.INPUT_RIGHT);
                    up = WasReleased(index, Global.INPUT_UP);
                    down = WasReleased(index, Global.INPUT_DOWN);
                    break;
                case 3:
                    _left = WasBeingPressed(index, Global.INPUT_LEFT);
                    _right = WasBeingPressed(index, Global.INPUT_RIGHT);
                    up = WasBeingPressed(index, Global.INPUT_UP);
                    down = WasBeingPressed(index, Global.INPUT_DOWN);
                    break;
            }

            bool left = _left;
            if (InputSide < 0) left = _right;

            bool right = _right;
            if (InputSide < 0) right = _left;
            
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

        /*public void Serialize(BinaryWriter bw)
        {
            h.Serialize(bw);
            v.Serialize(bw);
            b1.Serialize(bw);
            b2.Serialize(bw);
            b3.Serialize(bw);
            b4.Serialize(bw);
        }
        public void Deserialize(BinaryReader br)
        {
            h.Deserialize(br);
            v.Deserialize(br);
            b1.Deserialize(br);
            b2.Deserialize(br);
            b3.Deserialize(br);
            b4.Deserialize(br);
        }
        public override int GetHashCode()
        {
            int hashCode = 1858597544;
            hashCode = hashCode * -1521134295 + h.GetHashCode();
            hashCode = hashCode * -1521134295 + v.GetHashCode();
            hashCode = hashCode * -1521134295 + b1.GetHashCode();
            hashCode = hashCode * -1521134295 + b2.GetHashCode();
            hashCode = hashCode * -1521134295 + b3.GetHashCode();
            hashCode = hashCode * -1521134295 + b4.GetHashCode();
            return hashCode;
        }*/
    }
}

[System.Serializable]
public struct InputRegistry
{
    public ushort rawInput;
    public ushort duration;

    public bool IsNull => rawInput == 0;

    /*public override string ToString()
    {
        return $"({h}, {v}, {b_a}, {b_b}, {b_c}, {b_d}, {duration})";
    }*/
};
