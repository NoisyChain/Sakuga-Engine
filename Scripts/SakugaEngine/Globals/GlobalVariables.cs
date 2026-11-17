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
        public const int DefaultGravity = 3800;
        public const int DefaultAcceleration = 6000;
        public const int DefaultDeceleration = 4200;
        public const int DefaultFriction = 6700;
        public const int GravityDecay = 2500;
        public const int HitstunDecayMinCombo = 8;

        public const uint MinHitstun = 8;
        public const uint DefaultBaseHitstop = 10;
        public const uint DefaultCounterHitstop = 14;
        public const uint DefaultClashHitstop = 20;
        public const uint DefaultArmorHitstop = 20;
        public const uint DefaultBlockHitstop = 10;
        public const uint DefaultThrowHitstop = 16;
        public const uint DefaultLongThrowHitstop = 30;
        public const uint FinalBlowHitstop = 60;
        public const int DefaultSelfSuperGaugeGain = 250;
        public const int DefaultOpponentSuperGaugeGain = 100;
        public const int DefaultKnockbackDuration = 6;
        public const int ThrowEscapePushForce = 45000;
        public const ushort BaseMinDamageScaling = 10;
        public const ushort BaseMaxDamageScaling = 100;
        public const ushort CornerMinDamageScaling = 15;
        public const ushort CornerMaxDamageScaling = 110;
        public const uint GuardCrushHitstun = 40;

        public const int DefaultHitVFX = 0;
        public const int DefaultBlockVFX = 1;
        public const int DefaultClashVFX = 2;
        public const int DefaultArmorVFX = 3;
        public const int DefaultGuardCrushVFX = 3;

        public const uint MoveBufferLength = 10;
        public const uint NotificationDuration = 60;
    }
}