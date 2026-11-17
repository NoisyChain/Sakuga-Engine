using Godot;
using SakugaEngine;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class DataContainer : Resource
    {
        [Export] public FighterProfile Profile;
        [Export] public FighterState[] States;
        [Export] public FighterStance[] Stances;

        [Export] public int MaxHealth = 10000;
        [Export] public int MaxSuperGauge = 10000;
        [Export] public int Weight = 100;
        [Export] public int MaxBurst = 10000;
        [Export] public int StartingBurst = 5000;
        [Export] public int BurstFillRate = 3;
        [Export] public Vector2I PressureRange = new Vector2I(-1000, 3000);
        [Export] public ushort BaseAttack = 100;
        [Export] public ushort BaseDefense = 100;

        [Export] public Godot.Collections.Dictionary<string, long> CustomConstants { get; private set; }

        [Export] public AIData _aiData;

        [Export] public FighterIntro[] Intros;
        [Export] public FighterOutro[] Outros;
        [Export] public FighterIntro[] VictoryPoses;
        [Export] public int DefeatPose;

        public FighterIntro GetIntro(string opponentName)
        {
            if (Intros == null || Intros.Length == 0) return null;

            if (Intros.Length == 1)
            {
                return Intros[0];
            }

            FighterIntro introToPlay = null;

            for (int i = 0; i < VictoryPoses.Length; i++)
            {
                if (VictoryPoses[i].ForOpponent == opponentName)
                {
                    introToPlay = VictoryPoses[i];
                    break;
                }
                else if (VictoryPoses[i].ForOpponent == "") introToPlay = VictoryPoses[i];
            }

            return introToPlay;
        }

        public FighterOutro GetOutro(string opponentName, out string victoryMessage)
        {
            victoryMessage = $"{Profile.ShortName} is winner!";
            if (Outros == null || Outros.Length == 0) return null;

            if (Outros.Length == 1)
            {
                victoryMessage = Outros[0].FinalMessage;
                return Outros[0];
            }

            FighterOutro outroToPlay = null;

            for (int i = 0; i < Outros.Length; i++)
            {
                if (Outros[i].ForOpponent == opponentName)
                {
                    outroToPlay = Outros[i];
                    break;
                }
                else if (Outros[i].ForOpponent == "") outroToPlay = Outros[i];
            }

            if (outroToPlay != null)
                victoryMessage = outroToPlay.FinalMessage;
            return outroToPlay;
        }

        public FighterIntro GetVictoryPose(string opponentName)
        {
            if (VictoryPoses == null || VictoryPoses.Length == 0) return null;

            if (VictoryPoses.Length == 1)
            {
                return VictoryPoses[0];
            }

            FighterIntro introToPlay = null;

            for (int i = 0; i < VictoryPoses.Length; i++)
            {
                if (VictoryPoses[i].ForOpponent == opponentName)
                {
                    introToPlay = VictoryPoses[i];
                    break;
                }
                else if (VictoryPoses[i].ForOpponent == "") introToPlay = VictoryPoses[i];
            }

            return introToPlay;
        }

        public long GetCustomConstant(string key) => CustomConstants[key];
    }
}
