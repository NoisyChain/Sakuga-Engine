using System;

namespace SakugaEngine.Global
{
    [Flags]
    public enum PlayerInputs : ushort
    {
        NEUTRAL = 0,
        UP = 1 << 0,
        DOWN = 1 << 1,
        LEFT = 1 << 2,
        RIGHT = 1 << 3,
        FACE_A = 1 << 4,
        FACE_B = 1 << 5,
        FACE_C = 1 << 6,
        FACE_D = 1 << 7,
        FACE_E = 1 << 8,
        FACE_F = 1 << 9,
        FACE_G = 1 << 10,
        FACE_H = 1 << 11,

        //Free space
        
        MENU = 1 << 14,
        BACK = 1 << 15,
        ANY_DIRECTION = UP | DOWN | LEFT | RIGHT,
        ANY_BUTTON = FACE_A | FACE_B | FACE_C | FACE_D | FACE_E | FACE_F | FACE_G | FACE_H,
    }
    [Flags]
    public enum BlockType : byte
    { 
        HIGH = 1 << 0,
        MEDIUM = 1 << 1,
        LOW = 1 << 2
    }
    [Flags]
    public enum DirectionalInputs : ushort
    {
        DOWN = 1 << 0,
        LEFT = 1 << 1,
        RIGHT = 1 << 2,
        UP = 1 << 3,
        DOWN_LEFT = DOWN | LEFT,
        DOWN_RIGHT = DOWN | RIGHT,
        UP_LEFT = UP | LEFT,
        UP_RIGHT = UP | RIGHT,
        ANY_DIRECTION = DOWN | LEFT | RIGHT | UP
    }

    [Flags]
    public enum ButtonInputs : ushort
    {
        FACE_A = 1 << 0,
        FACE_B = 1 << 1,
        FACE_C = 1 << 2,
        FACE_D = 1 << 3,
        FACE_E = 1 << 4,
        FACE_F = 1 << 5,
        FACE_G = 1 << 6,
        FACE_H = 1 << 7,
        ANY_BUTTON = FACE_A | FACE_B | FACE_C | FACE_D | FACE_E | FACE_F | FACE_G | FACE_H
    }

    [Flags]
    public enum HitChecker : byte
    {
        NONE = 0,
        MASTER = 1 << 0,
        ALLIES = MASTER | 1 << 1,
        OPPONENTS = 1 << 2
    }
    [Flags]
    public enum FrameProperties : byte
    {
        DAMAGE_IMUNITY = 1 << 0,
        THROW_IMUNITY = 1 << 1,
        PROJECTILE_IMUNITY = 1 << 2,
        LOCK_MOVE = 1 << 4
    }
    [Flags]
    public enum TransitionCondition : byte
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
    [Flags]
    public enum AIFlags : ushort
    {
        IDLE = 1 << 0,
        GROUND_ACTION = 1 << 1,
        AIR_ACTION = 1 << 2,
        MOVEMENT_STATE = 1 << 3,
        ATTACK_STATE = 1 << 4,
        HITSTUN_STATE = 1 << 5,
        BLOCKSTUN_STATE = 1 << 6,
        KNOCKED_DOWN = 1 << 7,
        HIGH_ACTION = 1 << 8,
        LOW_ACTION = 1 << 9,
        CLOSE_ACTION = 1 << 10,
        FAR_ACTION = 1 << 11,
        INVULNERABLE = 1 << 12,
    }
    [Flags]
    public enum CancelCondition : byte
    {
        WHIFF_CANCEL = 1 << 0,
        HIT_CANCEL = 1 << 1,
        BLOCK_CANCEL = 1 << 2,
        KARA_CANCEL = 1 << 3
    }
}
