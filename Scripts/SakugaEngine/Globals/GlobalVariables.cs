namespace SakugaEngine.Global
{
    public static class GlobalVariables
    {
        //Global settings
        public const int TicksPerSecond = 60;
        public const int SubSteps = 2; // Discrete substeps for collisions
        public const int Delta = TicksPerSecond * SubSteps;
        public const int SimulationScale = 10000; //World units multiplied by this
        public const int WallLimit = 75000;
        public const int CeilingLimit = 120000;
        public const int StartingPosition = 12500;
        public const int MaxPlayersDistance = 70000;
        public const int InputHistorySize = 16;
        public const int KaraCancelWindow = 3;
        public const int InstantBlockWindow = 3;
        public const int DefaultGravity = 230000;
        public const int DefaultAcceleration = 360000;
        public const int DefaultDeceleration = 250000;
        public const int DefaultFriction = 400000;
        public const int GravityDecay = 2500;
        public const int HitstunDecayMinCombo = 8;
        public const int MinHitstun = 8;
        public const int DefaultBaseHitstop = 10;
        public const int DefaultCounterHitstop = 20;
        public const int DefaultClashHitstop = 20;
        public const int DefaultArmorHitstop = 20;
        public const int DefaultBlockHitstop = 6;
        public const int DefaultThrowHitstop = 16;
        public const int DefaultLongThrowHitstop = 30;
        public const int ThrowEscapePushForce = 45000;
        public const int GuardCrushHitstun = 40;
        public const ushort BaseMinDamageScaling = 35;
        public const ushort BaseMaxDamageScaling = 100;
        public const ushort CornerMinDamageScaling = 45;
        public const ushort CornerMaxDamageScaling = 120;
        public const int MoveBufferLength = 10;
        public const int NotificationDuration = 60;
    }
}