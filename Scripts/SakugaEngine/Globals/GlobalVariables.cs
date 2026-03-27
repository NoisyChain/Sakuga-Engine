namespace SakugaEngine.Global
{
    public static class GlobalVariables
    {
        //Global settings
        public const int TicksPerSecond = 60;
        public const int SubSteps = 2;
        public const int Delta = TicksPerSecond * SubSteps;
        public const int SimulationScale = 10000;
        public const int WallLimit = 75000;
        public const int CeilingLimit = 120000;
        public const int StartingPosition = 12000;
        public const int MaxPlayersDistance = 70000;
        public const int InputHistorySize = 16;
        public const int KaraCancelWindow = 3;
        public const int DefaultGravity = 200000;
        public const int DefaultAcceleration = 200000;
        public const int DefaultDeceleration = 150000;
        public const int DefaultFriction = 300000;
        public const int GravityDecay = 2500;
        public const int HitstunDecayMinCombo = 8;
        public const int MinHitstun = 8;
        public const int RecoveryJumpVelocity = 60000;
        public const int RecoveryGravity = 200000;
        public const int RecoveryHorizontalSpeed = 45000;
        public const int ThrowEscapePushForce = 45000;
        public const int GuardCrushHitstun = 40;
        public const ushort BaseMinDamageScaling = 35;
        public const ushort BaseMaxDamageScaling = 100;
        public const ushort CornerMinDamageScaling = 45;
        public const ushort CornerMaxDamageScaling = 120;
        public const int MoveBufferLength = 10;
        public const int DefaultVFXSoundChannel = 0;

        /*//Global inputs
        public const int INPUT_UP = 1 << 0;
        public const int INPUT_DOWN = 1 << 1;
        public const int INPUT_LEFT = 1 << 2;
        public const int INPUT_RIGHT = 1 << 3;
        public const int INPUT_FACE_A = 1 << 4;
        public const int INPUT_FACE_B = 1 << 5;
        public const int INPUT_FACE_C = 1 << 6;
        public const int INPUT_FACE_D = 1 << 7;
        public const int INPUT_FACE_E = 1 << 8;
        public const int INPUT_FACE_F = 1 << 9;
        public const int INPUT_FACE_G = 1 << 10;
        public const int INPUT_FACE_H = 1 << 11;
        //Free space
        public const int INPUT_MENU = 1 << 14;
        public const int INPUT_BACK = 1 << 15;
        public const int INPUT_ANY_DIRECTION = INPUT_UP | INPUT_DOWN | INPUT_LEFT | INPUT_RIGHT;
        public const int INPUT_ANY_BUTTON = INPUT_FACE_A | INPUT_FACE_B | INPUT_FACE_C | INPUT_FACE_D | INPUT_FACE_E | INPUT_FACE_F | INPUT_FACE_G | INPUT_FACE_H;*/

        
    }
}