using System;
using Godot;

namespace SakugaEngine
{
    public static class Global
    {
        //Global settings
        public const int TicksPerSecond = 60;
        public const int SubSteps = 4;
        public const int Delta = TicksPerSecond * SubSteps;
        public const int SimulationScale = 10000;
        public const int WallLimit = 75000;
        public const int CeilingLimit = 120000;
        public const int StartingPosition = 15000;
        public const int MaxPlayersDistance = 70000;
        public const int InputHistorySize = 16;
        public const int KaraCancelWindow = 3;
        public const int RoundsToWin = 2; //<= Move this to the GameMonitor
        public const int GameTimer = 99;  //<= Move this to the GameMonitor
        public const int MinHitstun = 8;
        public const int HitGrabStunAdditional = 8;

        //Global inputs
        public const int INPUT_UP = 1 << 0;
        public const int INPUT_DOWN = 1 << 1;
        public const int INPUT_LEFT = 1 << 2;
        public const int INPUT_RIGHT = 1 << 3;
        public const int INPUT_FACE_A = 1 << 4;
        public const int INPUT_FACE_B = 1 << 5;
        public const int INPUT_FACE_C = 1 << 6;
        public const int INPUT_FACE_D = 1 << 7;
        //Free space
        public const int INPUT_MACRO_DASH = 1 << 12;
        public const int INPUT_TAUNT = 1 << 13;
        public const int INPUT_MENU = 1 << 14;
        public const int INPUT_BACK = 1 << 15;
        public const int INPUT_ANY_DIRECTION = INPUT_UP | INPUT_DOWN | INPUT_LEFT | INPUT_RIGHT;
        public const int INPUT_ANY_BUTTON = INPUT_FACE_A | INPUT_FACE_B | INPUT_FACE_C | INPUT_FACE_D;

        //Global enumerators
        public enum AnimationStage { STARTUP, ACTIVE, RECOVERY }
        public enum ButtonMode { PRESS, HOLD, RELEASE, UNPRESSED }
        public enum MoveEndCondition { STATE_END, RELEASE_BUTTON, STATE_TYPE_CHANGE }
        public enum SideChangeMode { NONE, CHANGE_SIDE, INTERRUPT }
        public enum StateType { NULL, MOVEMENT, COMBAT, BLOCKING, HIT_REACTION }
        public enum HitboxType{ HURTBOX, HITBOX, PROXIMITY_BLOCK, PROJECTILE, THROW, COUNTER, DEFLECT, PARRY }
        public enum HitType{ HIGH, MEDIUM, LOW, UNBLOCKABLE }
        public enum HitstunType { BASIC = 1, KNOCKDOWN = 2, STAGGER = 3 }
        public enum ExtraVariableMode { IDLE, INCREASE, DECREASE }
        public enum ExtraVariableChange { SET, ADD, SUBTRACT }
        public enum ExtraVariableCompareMode { EQUAL, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL }
        public enum SoundType{ SFX, VOICE }
        public enum FadeScreenMode { NONE, FADE_IN, FADE_OUT }
        public enum EventType{ SPAWN_OBJECT, TELEPORT, ANIMATION_DAMAGE, DETTACH_THROW, FORCE_SIDE_CHANGE, SET_SUPER_ARMOR }
        public enum RelativeTo{ WORLD, SELF, FIGHTER, SPAWNABLE }
        public enum ObjectType{ SPAWNABLE, VXF }
        public enum SpawnableHitCheck{ OPPONENT, OWNER, BOTH }
        public enum PauseMode { PRESS, HOLD, LOCK }
        //...
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

        //Global bit flags
        [Flags] public enum FrameProperties : byte
        {
            DAMAGE_IMUNITY = 1 << 0,
            THROW_IMUNITY = 1 << 1,
            PROJECTILE_IMUNITY = 1 << 2,
            FORCE_MOVE_CANCEL = 1 << 3
        }
        [Flags] public enum TransitionCondition : byte
        {
            STATE_END = 1 << 0,
            AT_FRAME = 1 << 1,
            ON_GROUND = 1 << 2,
            ON_WALLS = 1 << 3,
            ON_FALL = 1 << 4,
            ON_LIFE_END = 1 << 5,
            ON_INPUT_COMMAND = 1 << 6,
            ON_DISTANCE = 1 << 7,
        }

        //Random Number Generator
        public static Random RNG;
        public static string baseSeed = "Sakuga Engine"; //You can change this if you want
        public static void UpdateRNG(int seed)
        {
            RNG = new Random(seed);
        }

        public static Vector2 ToScaledVector2(Vector2I vector)
        {
            return new Vector2
            (
                vector.X / (float)SimulationScale,
                vector.Y / (float)SimulationScale
            );
        }
        public static Vector3 ToScaledVector3(Vector2I vector, float zScale = 0f)
        {
            return new Vector3
            (
                vector.X / (float)SimulationScale,
                vector.Y / (float)SimulationScale,
                zScale
            );
        }
        public static Vector3 ToScaledVector3(Vector3I vector)
        {
            return new Vector3
            (
                vector.X / (float)SimulationScale,
                vector.Y / (float)SimulationScale,
                vector.Z / (float)SimulationScale
            );
        }
        public static float ToScaledFloat(int value)
        {
            return value / (float)SimulationScale;
        }

        public static int IntLerp(int from, int to, int numberOfSteps, int currentStep)
        {
            return ((to - from) * currentStep) / numberOfSteps;
        }

        public static bool IsOnRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static int HorizontalDistance(Vector2I a, Vector2I b)
        {
            int ax = a.X - b.X;
            if (ax == 0) return 0;
            
            int dx = ax / ax;
            //int dy = a.Y - b.Y;
            int dy = 0;
            return dx * dx + dy * dy;
        }
    }
}