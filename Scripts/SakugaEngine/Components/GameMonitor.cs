using System.IO;
using Godot;

namespace SakugaEngine
{
    public partial class GameMonitor : Node
    {
        [Export] public int ClockLimit = 99;
        [Export] public int RoundsToWin = 2;
        public int Clock;
        public int CurrentRound;
        public int Winner;
        public bool GameIsRunning;
        public bool RoundIsRunning;

        public int[] VictoryCounter;

        public Global.FadeScreenMode FadeMode;
        public int FadeScreenIntensity;
        public int FadeTime;
        public int FadeProgress;

        private SakugaFighter[] _fighters;

        public void Initialize(SakugaFighter[] fighters)
        {
            _fighters = fighters;
            Winner = -1;
            CurrentRound = 0;
            Clock = ClockLimit * Global.TicksPerSecond;
            GameIsRunning = true;
            RoundIsRunning = true; //<< This will be useful soon
            VictoryCounter = new int[_fighters.Length];
            for (int i = 0; i < VictoryCounter.Length; ++i)
            {
                VictoryCounter[i] = 0;
            }
            FadeScreenIntensity = 100;
        }

        public void CheckVictoryConditions()
        {
            int biggerHealth = 0;

            //Do not compare the current healths directly to not mess up with characters
            //with different base health values.
            //Convert them into percentages instead... but a bit bigger for extra precision
            int p1HealthPercentage = _fighters[0].Variables.CurrentHealth * 1000 / _fighters[0].Variables.MaxHealth;
            int p2HealthPercentage = _fighters[1].Variables.CurrentHealth * 1000 / _fighters[1].Variables.MaxHealth;
            
            if (_fighters[1].IsKO() && _fighters[biggerHealth].IsKO())
                biggerHealth = -1; //Double K.O.
            else if (p2HealthPercentage == p1HealthPercentage)
                biggerHealth = _fighters.Length; //Draw
            else if (p2HealthPercentage > p1HealthPercentage)
                biggerHealth = 1; //Select a winner

            if (biggerHealth < 0)
            {
                GD.Print("Double K.O....");
            }
            else if (biggerHealth >= _fighters.Length)
            {
                GD.Print("Draw...");
            }
            else
            {
                VictoryCounter[biggerHealth]++;
                GD.Print($"Player {biggerHealth + 1} win the round {CurrentRound + 1}!");
                if (VictoryCounter[biggerHealth] == RoundsToWin)
                    Winner = biggerHealth;
            }

            GameIsRunning = false;

            if (Winner < 0) 
            {
                for (int i = 0; i < _fighters.Length; i++)
                {
                    _fighters[i].Reset(i);
                }
                Reset();
            }
            else GD.Print($"Player {Winner + 1} win the game!");
        }

        public void Tick()
        {
            if (!GameIsRunning) return;

            bool p1TimeStop = _fighters[0].SuperFlash || _fighters[0].CinematicState;
            bool p2TimeStop = _fighters[1].SuperFlash || _fighters[1].CinematicState;

            FadeController();

            if (RoundIsRunning && !p1TimeStop && !p2TimeStop)
            {
                if (Clock >= 0)
                {
                    Clock--;

                    //Check if someone is KO'ed
                    bool someoneLose = false;
                    for (int i = 0; i < _fighters.Length; i++)
                    {
                        if (_fighters[i].IsKO())
                            someoneLose = true;
                    }
                    if (someoneLose)
                        CheckVictoryConditions();
                }
                else
                    CheckVictoryConditions();
            }
        }

        public void Reset()
        {
            CurrentRound++;
            Clock = ClockLimit * Global.TicksPerSecond;
            GameIsRunning = true;
        }

        public void FadeIn(int time)
        {
            FadeTime = time;
            FadeProgress = FadeTime;
            FadeMode = Global.FadeScreenMode.FADE_IN;
        }

        public void FadeOut(int time)
        {
            FadeTime = time;
            FadeProgress = 0;
            FadeMode = Global.FadeScreenMode.FADE_OUT;
        }

        public void FadeController()
        {
            if (FadeMode == Global.FadeScreenMode.NONE) return;

            if ((FadeMode == Global.FadeScreenMode.FADE_IN && FadeProgress <= 0) ||
                (FadeMode == Global.FadeScreenMode.FADE_OUT && FadeProgress >= FadeTime))
                FadeMode = Global.FadeScreenMode.NONE;

            switch ((int)FadeMode)
            {
                case 1:
                    FadeProgress--;
                    break;
                case 2:
                    FadeProgress++;
                    break;
            }
            if (FadeTime > 0) FadeScreenIntensity = Global.IntLerp(0, 100, FadeTime, FadeProgress);
        }

        public void Serialize(BinaryWriter bw) 
        {
            bw.Write((byte)FadeMode);

            bw.Write(Clock);
            bw.Write(CurrentRound);
            bw.Write(Winner);
            bw.Write(FadeScreenIntensity);
            bw.Write(FadeTime);
            bw.Write(FadeProgress);

            bw.Write(GameIsRunning);
            bw.Write(RoundIsRunning);

            for (int i = 0; i < VictoryCounter.Length; ++i)
            {
                bw.Write(VictoryCounter[i]);
            }
        }

        public void Deserialize(BinaryReader br) 
        {
            FadeMode = (Global.FadeScreenMode)br.ReadByte();

            Clock = br.ReadInt32();
            CurrentRound = br.ReadInt32();
            Winner = br.ReadInt32();
            FadeScreenIntensity = br.ReadInt32();
            FadeTime = br.ReadInt32();
            FadeProgress = br.ReadInt32();

            GameIsRunning = br.ReadBoolean();
            RoundIsRunning = br.ReadBoolean();
            
            for (int i = 0; i < VictoryCounter.Length; ++i)
            {
                VictoryCounter[i] = br.ReadInt32();
            }
        }
    }
}
