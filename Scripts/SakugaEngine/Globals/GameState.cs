using Godot;
using MessagePack;
using SakugaEngine.Global;
using SakugaEngine.Collision;
using System.Collections.Generic;

namespace SakugaEngine.GameState
{
    [MessagePackObject]
    public struct SakugaActorState
    {
        [Key(0)] public bool IsActive;
        [Key(1)] public PhysicsBodyState Body;
        [Key(2)] public ParametersState Parameters;
        [Key(3)] public StateManagerState StateManager;
        [Key(4)] public StanceManagerState StanceManager;
        [Key(5)] public int LayerSorting;
        [Key(6)] public int BounceXIntensity;
        [Key(7)] public int BounceYIntensity;
        [Key(8)] public bool SuperFlashing;
        [Key(9)] public bool CinematicState;
        [Key(10)] public bool BlockStun;
        [Key(11)] public HitstunType HitstunType;
        [Key(12)] public FrameProperties FrameProperties;
        [Key(13)] public HitChecker HitCheck;
        [Key(14)] public CancelCondition CancelConditions;

        public void GetStateData(ref SakugaActor reference)
        {
            IsActive = reference.IsActive;

            Body.GetStateData(ref reference.Body);
            Parameters.GetStateData(ref reference.Parameters);
            StateManager.GetStateData(ref reference.StateManager);
            StanceManager.GetStateData(ref reference.StanceManager);

            LayerSorting = reference.LayerSorting;
            BounceXIntensity = reference.LayerSorting;
            BounceYIntensity = reference.LayerSorting;
            SuperFlashing = reference.SuperFlashing;
            CinematicState = reference.CinematicState;
            BlockStun = reference.BlockStun;
            HitstunType = reference.HitstunType;
            FrameProperties = reference.FrameProperties;
            HitCheck = reference.HitCheck;
            CancelConditions = reference.CancelConditions;
        }

        public void SetStateData(ref SakugaActor reference)
        {
            reference.IsActive = IsActive;

            Body.SetStateData(ref reference.Body);
            Parameters.SetStateData(ref reference.Parameters);
            StateManager.SetStateData(ref reference.StateManager);
            StanceManager.SetStateData(ref reference.StanceManager);

            reference.LayerSorting = LayerSorting;
            reference.BounceXIntensity = LayerSorting;
            reference.BounceYIntensity = LayerSorting;
            reference.SuperFlashing = SuperFlashing;
            reference.CinematicState = CinematicState;
            reference.BlockStun = BlockStun;
            reference.HitstunType = HitstunType;
            reference.FrameProperties = FrameProperties;
            reference.HitCheck = HitCheck;
            reference.CancelConditions = CancelConditions;
        }
    }

    [MessagePackObject]
    public struct SakugaVFXState
    {
        [Key(0)] public bool IsActive;
        [Key(1)] public bool AttachedToOwner;
        [Key(2)] public Vector2IState FixedPosition;
        [Key(3)] public int Frame;
        [Key(4)] public int Side;

        public void GetStateData(ref SakugaVFX reference)
        {
            if (reference == null) return;
            
            IsActive = reference.IsActive;
            AttachedToOwner = reference.AttachedToOwner;
            FixedPosition.GetStateData(ref reference.FixedPosition);
            Frame = reference.Frame;
            Side = reference.Side;
        }

        public void SetStateData(ref SakugaVFX reference)
        {
            if (reference == null) return;
            
            reference.IsActive = IsActive;
            reference.AttachedToOwner = AttachedToOwner;
            FixedPosition.SetStateData(ref reference.FixedPosition);
            reference.Frame = Frame;
            reference.Side = Side;
        }
    }

    [MessagePackObject]
    public struct GameManagerState
    {
        [Key(0)] public InputManagerState[] Inputs;
        [Key(1)] public int Frame;
        [Key(2)] public GameMonitorState GameMonitor;
        [Key(3)] public byte[][] SakugaNodes;
        [Key(4)] public Vector3IState RandomTest;

        public void GetStateData(ref Game.GameManager reference)
        {
            if (reference == null) return;
            
            Inputs = new InputManagerState[reference.Inputs.Length];
            for (int i = 0; i < reference.Inputs.Length; i++)
            {
                Inputs[i].GetStateData(ref reference.Inputs[i]);
            }
            Frame = reference.Frame;
            GameMonitor.GetStateData(ref reference.Monitor);
            SakugaNodes = new byte[reference.Nodes.Count][];
            for(int i = 0; i < reference.Nodes.Count; i++)
            {
                SakugaNodes[i] = reference.Nodes[i].GetStateData();
            }
            RandomTest.GetStateData(ref reference.randomTest);
        }

        public void SetStateData(ref Game.GameManager reference)
        {
            if (reference == null) return;
            
            for (int i = 0; i < reference.Inputs.Length; i++)
            {
                Inputs[i].SetStateData(ref reference.Inputs[i]);
            }
            reference.Frame = Frame;
            GameMonitor.SetStateData(ref reference.Monitor);
            for(int i = 0; i < reference.Nodes.Count; i++)
            {
                reference.Nodes[i].SetStateData(SakugaNodes[i]);
            }
            RandomTest.SetStateData(ref reference.randomTest);
        }
    }

    [MessagePackObject]
    public struct PhysicsBodyState
    {
        [Key(0)] public Vector2IState FixedPosition;
        [Key(1)] public Vector2IState FixedVelocity;
        [Key(2)] public bool IsLeftSide;
        [Key(3)] public int PlayerSide;
        [Key(4)] public bool IsMovable;
        [Key(5)] public List<uint> HitBodies;
        [Key(6)] public bool ProximityBlocked;
        [Key(7)] public int CurrentGravity;

        public void GetStateData(ref PhysicsBody reference)
        {
            if (reference == null) return;

            FixedPosition.GetStateData(ref reference.FixedPosition);
            FixedVelocity.GetStateData(ref reference.FixedVelocity);
            IsLeftSide = reference.IsLeftSide;
            PlayerSide = reference.PlayerSide;
            IsMovable = reference.IsMovable;
            HitBodies = reference.HitBodies;
            ProximityBlocked = reference.ProximityBlocked;
            CurrentGravity = reference.CurrentGravity;
        }

        public void SetStateData(ref PhysicsBody reference)
        {
            if (reference == null) return;
            
            FixedPosition.SetStateData(ref reference.FixedPosition);
            FixedVelocity.SetStateData(ref reference.FixedVelocity);
            reference.IsLeftSide = IsLeftSide;
            reference.PlayerSide = PlayerSide;
            reference.IsMovable = IsMovable;
            reference.HitBodies = HitBodies;
            reference.ProximityBlocked = ProximityBlocked;
            reference.CurrentGravity = CurrentGravity;
        }
    }

    [MessagePackObject]
    public struct ParametersState
    {
        [Key(0)] public int CurrentHealth;
        [Key(1)] public int LostHealth;
        [Key(2)] public int RecoveryHealth;
        [Key(3)] public int CurrentSuperGauge;
        [Key(4)] public int CurrentSuperArmor;
        [Key(5)] public ProrationsState Prorations;
        [Key(6)] public CombatTrackerState CombatTracker;
        [Key(50)] public FrameTimerState[] Timers;
        [Key(51)] public CustomVariableState[] Variables;
        public void GetStateData(ref SakugaParameters reference)
        {
            if (reference == null) return;

            if (reference.Health != null)
            {
                CurrentHealth = reference.Health.CurrentValue;
                LostHealth = reference.Health.LostValue;
                RecoveryHealth = reference.Health.RecoverableValue;
            }

            if (reference.SuperGauge != null) CurrentSuperGauge = reference.SuperGauge.CurrentValue;
            if (reference.SuperArmor != null) CurrentSuperArmor = reference.SuperArmor.CurrentValue;

            if (reference.Prorations != null) Prorations.GetStateData(ref reference.Prorations);
            if (reference.Tracker != null) CombatTracker.GetStateData(ref reference.Tracker);
            
            Timers = new FrameTimerState[reference.Timers.Length];
            for (int t = 0; t < reference.Timers.Length; t++)
            {
                Timers[t].GetStateData(ref reference.Timers[t]);
            }
            Variables = new CustomVariableState[reference.Variables.Length];
            for (int t = 0; t < reference.Variables.Length; t++)
            {
                Variables[t].GetStateData(ref reference.Variables[t]);
            }
        }

        public void SetStateData(ref SakugaParameters reference)
        {
            if (reference == null) return;
            
            if (reference.Health != null)
            {
                reference.Health.CurrentValue = CurrentHealth;
                reference.Health.LostValue = LostHealth;
                reference.Health.RecoverableValue = RecoveryHealth;
            }

            if (reference.SuperGauge != null) reference.SuperGauge.CurrentValue = CurrentSuperGauge;
            if (reference.SuperArmor != null) reference.SuperArmor.CurrentValue = CurrentSuperArmor;

            if (reference.Prorations != null) Prorations.SetStateData(ref reference.Prorations);
            if (reference.Tracker != null) CombatTracker.SetStateData(ref reference.Tracker);
            
            for (int t = 0; t < reference.Timers.Length; t++)
            {
                Timers[t].SetStateData(ref reference.Timers[t]);
            }
            for (int t = 0; t < reference.Variables.Length; t++)
            {
                Variables[t].SetStateData(ref reference.Variables[t]);
            }
        }
    }

    [MessagePackObject]
    public struct StateManagerState
    {
        [Key (0)] public int CurrentState;
        [Key (1)] public int CurrentStateFrame;

        public void GetStateData(ref StateManager reference)
        {
            if (reference == null) return;
            
            CurrentState = reference.CurrentState;
            CurrentStateFrame = reference.CurrentStateFrame;
        }

        public void SetStateData(ref StateManager reference)
        {
            if (reference == null) return;
            
            reference.CurrentState = CurrentState;
            reference.CurrentStateFrame = CurrentStateFrame;
        }
    }

    [MessagePackObject]
    public struct StanceManagerState
    {
        [Key (0)] public int CurrentStance;
        [Key (1)] public int BufferedMove;
        [Key (2)] public int CurrentMove;
        [Key (3)] public bool CancelBuffer;

        public void GetStateData(ref StanceManager reference)
        {
            if (reference == null) return;
            
            CurrentStance = reference.CurrentStance;
            BufferedMove = reference.BufferedMove;
            CurrentMove = reference.CurrentMove;
            CancelBuffer = reference.CancelBuffer;
        }

        public void SetStateData(ref StanceManager reference)
        {
            if (reference == null) return;
            
            reference.CurrentStance = CurrentStance;
            reference.BufferedMove = BufferedMove;
            reference.CurrentMove = CurrentMove;
            reference.CancelBuffer = CancelBuffer;
        }
    }

    [MessagePackObject]
    public struct ProrationsState
    {
        [Key(0)] public ushort CurrentAttack;
        [Key(1)] public ushort CurrentDefense;
        [Key(2)] public ushort CurrentDamageScaling;
        [Key(3)] public ushort CurrentBaseDamageScaling;
        [Key(4)] public ushort CurrentCornerDamageScaling;
        [Key(5)] public ushort CurrentSameMoveProration;
        [Key(6)] public ushort CurrentGravityProration;
        [Key(7)] public ushort CurrentHitstunProration;
        [Key(8)] public byte GravityDecayFactor;
        [Key(9)] public byte HitstunDecayFactor;

        public void GetStateData(ref SakugaProrations reference)
        {
            if (reference == null) return;

            CurrentAttack = reference.CurrentAttack;
            CurrentDefense = reference.CurrentDefense;
            CurrentDamageScaling = reference.CurrentDamageScaling;
            CurrentBaseDamageScaling = reference.CurrentBaseDamageScaling;
            CurrentCornerDamageScaling = reference.CurrentCornerDamageScaling;
            CurrentSameMoveProration = reference.CurrentSameMoveProration;
            CurrentGravityProration = reference.CurrentGravityProration;
            CurrentHitstunProration = reference.CurrentHitstunProration;
            GravityDecayFactor = reference.GravityDecayFactor;
            HitstunDecayFactor = reference.HitstunDecayFactor;
        }

        public void SetStateData(ref SakugaProrations reference)
        {
            if (reference == null) return;

            reference.CurrentAttack = CurrentAttack;
            reference.CurrentDefense = CurrentDefense;
            reference.CurrentDamageScaling = CurrentDamageScaling;
            reference.CurrentBaseDamageScaling = CurrentBaseDamageScaling;
            reference.CurrentCornerDamageScaling = CurrentCornerDamageScaling;
            reference.CurrentSameMoveProration = CurrentSameMoveProration;
            reference.CurrentGravityProration = CurrentGravityProration;
            reference.CurrentHitstunProration = CurrentHitstunProration;
            reference.GravityDecayFactor = GravityDecayFactor;
            reference.HitstunDecayFactor = HitstunDecayFactor;
        }
    }

    [MessagePackObject]
    public struct CombatTrackerState
    {
        [Key(0)] public int HitCombo;
        [Key(1)] public int LastDamage;
        [Key(2)] public int CurrentCombo;
        [Key(3)] public int HighestCombo;
        [Key(4)] public int TotalFrames;
        [Key(5)] public int FrameData;
        [Key(6)] public int FrameAdvantage;
        [Key(7)] public int HitFrame;
        [Key(8)] public int StunAtHit;
        [Key(9)] public int LastHitType;
        [Key(10)] public bool invalidHit;

        public void GetStateData(ref CombatTracker reference)
        {
            if (reference == null) return;

            HitCombo = reference.HitCombo;
            LastDamage = reference.LastDamage;
            CurrentCombo = reference.CurrentCombo;
            HighestCombo = reference.HighestCombo;
            TotalFrames = reference.TotalFrames;
            FrameData = reference.FrameData;
            FrameAdvantage = reference.FrameAdvantage;
            HitFrame = reference.HitFrame;
            StunAtHit = reference.StunAtHit;
            LastHitType = reference.LastHitType;
            invalidHit = reference.invalidHit;
        }

        public void SetStateData(ref CombatTracker reference)
        {
            if (reference == null) return;

            reference.HitCombo = HitCombo;
            reference.LastDamage = LastDamage;
            reference.CurrentCombo = CurrentCombo;
            reference.HighestCombo = HighestCombo;
            reference.TotalFrames = TotalFrames;
            reference.FrameData = FrameData;
            reference.FrameAdvantage = FrameAdvantage;
            reference.HitFrame = HitFrame;
            reference.StunAtHit = StunAtHit;
            reference.LastHitType = LastHitType;
            reference.invalidHit = invalidHit;
        }
    }

    [MessagePackObject]
    public struct FrameTimerState
    {
        [Key (0)] public uint WaitTime;
        [Key (1)] public uint TimeLeft;
        [Key (2)] public bool Paused;

        public void GetStateData(ref FrameTimer reference)
        {
            if (reference == null) return;
            
            WaitTime = reference.WaitTime;
            TimeLeft = reference.TimeLeft;
            Paused = reference.Paused;
        }

        public void SetStateData(ref FrameTimer reference)
        {
            if (reference == null) return;
            
            reference.WaitTime = WaitTime;
            reference.TimeLeft = TimeLeft;
            reference.Paused = Paused;
        }
    }

    [MessagePackObject]
    public struct CustomVariableState
    {
        [Key(0)] public int Value;
        [Key(1)] public int Factor;
        [Key(2)] public CustomVariableMode Mode;

        public void GetStateData(ref CustomVariable reference)
        {
            if (reference == null) return;
            
            Value = reference.CurrentValue;
            Factor = reference.CurrentFactor;
            Mode = reference.CurrentMode;
        }

        public void SetStateData(ref CustomVariable reference)
        {
            if (reference == null) return;
            
            reference.CurrentValue = Value;
            reference.CurrentFactor = Factor;
            reference.CurrentMode = Mode;
        }
    }

    [MessagePackObject]
    public struct GameMonitorState
    {
        [Key(1)] public FrameTimerState DelayTimer;
        [Key(2)] public int Clock;
        [Key(3)] public int CurrentRound;
        [Key(4)] public int Winner;
        [Key(0)] public FadeScreenMode FadeMode;
        [Key(5)] public int FadeScreenIntensity;
        [Key(6)] public int FadeTime;
        [Key(7)] public int FadeProgress;
        [Key(8)] public MatchState MatchState;
        [Key(9)] public int RoundWinner;
        [Key(10)] public int CardsCurrentAnimation;
        [Key(11)] public int CardsCurrentFrame;
        [Key(12)] public int[] VictoryCounter;

        public void GetStateData(ref GameMonitor reference)
        {
            if (reference == null) return;
            
            DelayTimer.GetStateData(ref reference.DelayTimer);
            Clock = reference.Clock;
            CurrentRound = reference.CurrentRound;
            Winner = reference.Winner;

            FadeMode = reference.FadeMode;
            FadeScreenIntensity = reference.FadeScreenIntensity;
            FadeTime = reference.FadeTime;
            FadeProgress = reference.FadeProgress;

            MatchState = reference.MatchState;
            CurrentRound = reference.CurrentRound;
            RoundWinner = reference.RoundWinner;
            CardsCurrentAnimation = reference.Cards.CurrentAnimation;
            CardsCurrentFrame = reference.Cards.Frame;

            VictoryCounter = reference.VictoryCounter;
        }

        public void SetStateData(ref GameMonitor reference)
        {
            if (reference == null) return;
            
            DelayTimer.SetStateData(ref reference.DelayTimer);
            reference.Clock = Clock;
            reference.CurrentRound = CurrentRound;
            reference.Winner = Winner;

            reference.FadeMode = FadeMode;
            reference.FadeScreenIntensity = FadeScreenIntensity;
            reference.FadeTime = FadeTime;
            reference.FadeProgress = FadeProgress;

            reference.MatchState = MatchState;
            reference.CurrentRound = CurrentRound;
            reference.RoundWinner = RoundWinner;
            reference.Cards.CurrentAnimation = CardsCurrentAnimation;
            reference.Cards.Frame = CardsCurrentFrame;
            
            reference.VictoryCounter = VictoryCounter;
        }
    }

    [MessagePackObject]
    public struct InputManagerState
    {
        [Key(0)] public PlayerInputs[] RawInputs;
        [Key(1)] public ushort[] Durations;
        [Key(2)] public short hCharge;
        [Key(3)] public short vCharge;
        [Key(4)] public int CurrentHistory;

        public void GetStateData(ref InputManager reference)
        {
            if (reference == null) return;
            
            RawInputs = new PlayerInputs[GlobalVariables.InputHistorySize];
            Durations = new ushort[GlobalVariables.InputHistorySize];
            for (int i = 0; i < GlobalVariables.InputHistorySize; i ++)
            {
                RawInputs[i] = reference.InputHistory[i].rawInput;
                Durations[i] = reference.InputHistory[i].duration;
            }
            hCharge = reference.hCharge;
            vCharge = reference.vCharge;
            CurrentHistory = reference.CurrentHistory;
        }

        public void SetStateData(ref InputManager reference)
        {
            if (reference == null) return;
            
            for (int i = 0; i < GlobalVariables.InputHistorySize; i ++)
            {
                reference.InputHistory[i].rawInput = RawInputs[i];
                reference.InputHistory[i].duration = Durations[i];
            }
            reference.hCharge = hCharge;
            reference.vCharge = vCharge;
            reference.CurrentHistory = CurrentHistory;
        }
    }

    [MessagePackObject]
    public struct Vector2IState
    {
        [Key(0)] public int X;
        [Key(1)] public int Y;

        public void GetStateData(ref Vector2I reference)
        {            
            X = reference.X;
            Y = reference.Y;
        }

        public void SetStateData(ref Vector2I reference)
        {            
            reference.X = X;
            reference.Y = Y;
        }
    }

    [MessagePackObject]
    public struct Vector3IState
    {
        [Key(0)] public int X;
        [Key(1)] public int Y;
        [Key(2)] public int Z;

        public void GetStateData(ref Vector3I reference)
        {
            X = reference.X;
            Y = reference.Y;
            Z = reference.Z;
        }

        public void SetStateData(ref Vector3I reference)
        {
            reference.X = X;
            reference.Y = Y;
            reference.Z = Z;
        }
    }
}
