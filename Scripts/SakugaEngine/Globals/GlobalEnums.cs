namespace SakugaEngine.Global
{
    public enum AnimationStage { STARTUP, ACTIVE, RECOVERY }
    public enum ButtonMode { PRESS, HOLD, RELEASE, WAS_PRESSED, NOT_PRESSED }
    public enum MoveEndCondition { STATE_END, RELEASE_BUTTON, STATE_TYPE_CHANGE, ON_FALL }
    public enum SideChangeMode { NONE, CHANGE_SIDE, INTERRUPT }
    public enum MasterStance { NEUTRAL, CROUCH, ON_THE_GROUND }
    public enum StanceSelect { GROUND, CROUCH, AIR }
    public enum StateType { NULL, MOVEMENT, COMBAT, BLOCKING, HIT_REACTION, LOCKED }
    public enum HitboxType { HURTBOX, HITBOX, PROXIMITY_BLOCK, PROJECTILE, THROW, COUNTER, DEFLECT }
    public enum HitType { HIGH, MEDIUM, LOW, UNBLOCKABLE }
    public enum HitstunType { NONE = 0, BASIC = 1, KNOCKDOWN = 2, HARD_KNOCKDOWN = 3, DIZZINESS = 4, STAGGERED = 5, GRABBED }
    public enum CustomVariableMode { IDLE, INCREASE, DECREASE }
    public enum CustomVariableBehaviorTarget { ON_HIT, ON_DAMAGE, ON_MOVE_ENTER, ON_MOVE_EXIT, ON_FULL, ON_EMPTY, ON_USE }
    public enum ParameterChange { SET, ADD, SUBTRACT }
    public enum CompareMode { EQUAL, DIFFERENT, GREATER, GREATER_EQUAL, LESS, LESS_EQUAL }
    public enum FadeScreenMode { NONE, FADE_IN, FADE_OUT }
    public enum RelativeTo { WORLD, SELF, OPPONENT, FIGHTER, SPAWNABLE }
    public enum SoundType { SFX, VOICE }
    public enum ObjectType { SPAWNABLE, VFX }
    public enum PauseMode { PRESS, HOLD, LOCK }
    public enum NetcodeMode { LOCAL, ONLINE, REPLAY }
    public enum BotDifficulty { BEGINNER, EASY, MEDIUM, HARD, VERY_HARD, PRO }
    public enum BotMode { ANY = -1, AGGRESSIVE, DEFENSIVE }
    public enum RoundNamingOrder { N, Nth }
    // Character Select enums
    public enum CharacterSelectStyle : byte { VERSUS, PLAYER1, PLAYER2 }
    public enum CharacterSelectMode : byte { CHARACTER_SELECT, STAGE_SELECT }
    public enum CharacterSelectState : byte { SELECTING_CHARACTER, SELECTING_COLOR, DONE }
    public enum CameraFocus : byte { CENTER, PLAYER1, PLAYER2 }
    public enum CameraSelectFocus : byte { CENTER, SELF, OPPONENT }
    public enum MatchState { INTRO, CATCHPHRASE, ROUND_START, ROUND_RUNNING, ROUND_END, ROUND_WINNER, ROUND_INTERLUDE, NEXT_ROUND_TRANSITION, MATCH_OUTRO, RESULTS }
}
