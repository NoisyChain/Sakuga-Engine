using System.IO;

namespace SakugaEngine
{
    public class GameMonitor
    {
        public int clockLimit;
        public int Clock;
        public int CurrentRound;
        public int Winner;
        public bool GameIsRunning;

        public int[] VictoryCounter;

        public Global.FadeScreenMode FadeMode;
        public int FadeScreenIntensity;
        public int FadeTime;
        public int FadeProgress;

        public GameMonitor(int clockTime, int playerCount)
        {
            clockLimit = clockTime;
            Winner = -1;
            CurrentRound = 0;
            Clock = clockTime * Global.TicksPerSecond;
            GameIsRunning = true;
            VictoryCounter = new int[playerCount];
            for (int i = 0; i < VictoryCounter.Length; ++i)
            {
                VictoryCounter[i] = 0;
            }
            FadeScreenIntensity = 100;
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

            bw.Write(VictoryCounter.Length);
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

            var length = br.ReadInt32();
            if (length != VictoryCounter.Length)
                VictoryCounter = new int[length];
            
            for (int i = 0; i < VictoryCounter.Length; ++i)
            {
                VictoryCounter[i] = br.ReadInt32();
            }
        }

        public void CheckVictoryConditions(FighterBody[] _fighters)
        {
            int biggerHealth = 0;

            //Do not compare the current healths directly to not mess up with characters
            //with different base health values.
            //Convert them into percentages instead... but a big bigger for extra precision
            uint p1HealthPercentage = _fighters[biggerHealth].Variables.CurrentHealth * 1000 / _fighters[biggerHealth].Variables.MaxHealth;
            uint p2HealthPercentage = _fighters[1].Variables.CurrentHealth * 1000 / _fighters[1].Variables.MaxHealth;
            
            if (_fighters[1].IsKO() && _fighters[biggerHealth].IsKO())
                biggerHealth = -1; //Double K.O.
            else if (p2HealthPercentage == p1HealthPercentage)
                biggerHealth = _fighters.Length; //Draw
            else if (p2HealthPercentage > p1HealthPercentage)
                biggerHealth = 1; //Select a winner

            if (biggerHealth < 0)
            {
                Godot.GD.Print("Double K.O....");
            }
            else if (biggerHealth >= _fighters.Length)
            {
                Godot.GD.Print("Draw...");
            }
            else
            {
                VictoryCounter[biggerHealth]++;
                Godot.GD.Print($"Player {biggerHealth + 1} win the round {CurrentRound + 1}!");
                if (VictoryCounter[biggerHealth] == Global.RoundsToWin)
                    Winner = biggerHealth;
            }

            GameIsRunning = false;

            if (Winner < 0) 
            {
                /*for (int i = 0; i < _fighters.Length; i++)
                {
                    _fighters[i].Reset(i);
                }*/
                Reset();
            }
            else Godot.GD.Print($"Player {Winner + 1} win the game!");
        }

        public void Tick(FighterBody[] _fighters)
        {
            if (!GameIsRunning) return;

            FadeController();

            if (Clock > 0)
            {
                Clock--;

                //Check if someone is KO'ed
                int someoneLose = 0;
                for (int i = 0; i < _fighters.Length; i++)
                {
                    if (_fighters[i].IsKO())
                        someoneLose++;
                }
                if (someoneLose > 0)
                    CheckVictoryConditions(_fighters);
            }
            else
                CheckVictoryConditions(_fighters);
        }

        public void Reset()
        {
            CurrentRound++;
            Clock = clockLimit * Global.TicksPerSecond;
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
            if (FadeTime > 0) FadeScreenIntensity = IntLerp(0, 100, FadeTime, FadeProgress);
        }

        public static int IntLerp(int from, int to, int numberOfSteps, int currentStep)
        {
            return ((to - from) * currentStep) / numberOfSteps;
        }
    }
}
