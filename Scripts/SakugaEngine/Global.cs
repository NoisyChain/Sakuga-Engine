using Godot;

public static class Global
{
    //Global settings
    public const int TicksPerSecond = 60;
    public const int SubSteps = 4;
    public const int Delta = TicksPerSecond * SubSteps;
    public const int SimulationScale = 10000;
    public const int WallLimit = 120000;
    public const int CeilingLimit = 70000;
    public const int StartingPosition = 15000;
    public const int InputHistorySize = 16;
    public const int KaraCancelWindow = 3;

    //Global inputs
    public const int INPUT_UP = 1 << 0;
    public const int INPUT_DOWN = 1 << 1;
    public const int INPUT_LEFT = 1 << 2;
    public const int INPUT_RIGHT = 1 << 3;
    public const int INPUT_FACE_A = 1 << 4;
    public const int INPUT_FACE_B = 1 << 5;
    public const int INPUT_FACE_C = 1 << 6;
    public const int INPUT_FACE_D = 1 << 7;

    //Global enumerators
    public enum ButtonMode { PRESS = 0, RELEASE = 1 }
    public enum MoveEndCondition { STATE_END = 0, RELEASE_BUTTON = 1, STATE_TYPE_CHANGE = 2 }
    public enum StateType { NULL, MOVEMENT, COMBAT, BLOCKING, HIT_REACTION }
    public enum TransitionCondition 
    {
        STATE_END = 0,
        AT_FRAME = 1,
        ON_GROUND = 2,
        ON_WALLS = 3,
        ON_FALL = 4,
        ON_KO = 5,
        ON_LIFETIME_END = 6,
        ON_INPUT_COMMAND = 7,
    }

    public enum DirectionalInputs
    {
        DOWN_LEFT = 1,
        DOWN = 2,
        DOWN_RIGHT = 3,
        LEFT = 4,
        NEUTRAL = 5,
        RIGHT = 6,
        UP_LEFT = 7,
        UP = 8,
        UP_RIGHT = 9,
        HORIZONTAL_CHARGE = 10,
        VERTICAL_CHARGE = 11,
        DIAGIONAL_CHARGE_UP = 12,
        DIAGIONAL_CHARGE_DOWN = 13
    }

    public enum ButtonInputs
    {
        NULL = 0,
        FACE_A = 1,
        FACE_B = 2,
        FACE_C = 3,
        FACE_D = 4,
        FACE_AB = 5,
        FACE_AC = 6,
        FACE_BC = 7,
        FACE_ABC = 8,
        FACE_ABCD = 9,
        FACE_ANY = 10,
        TAUNT = 11
    }

    //global functions
    public static Vector2 ToScaledVector2(Vector2I vector)
    {
        return new Vector2
        (
            vector.X / (float)SimulationScale,
            vector.Y / (float)SimulationScale
        );
    }
    public static Vector3 ToScaledVector3(Vector2I vector)
    {
        return new Vector3
        (
            vector.X / (float)SimulationScale,
            vector.Y / (float)SimulationScale,
            0f
        );
    }
    public static float ToScaledFloat(int value)
    {
        return value / (float)SimulationScale;
    }
}