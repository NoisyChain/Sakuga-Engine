using Godot;
using SakugaEngine.Resources;
using SakugaEngine.Global;
using SakugaEngine.UI;

namespace SakugaEngine
{
    public partial class GameMonitor : Node
    {
        [Export] public FrameTimer DelayTimer;
        public MatchCardsController Cards;
        public Control ResultsScreen;
        public int ClockLimit = 99;
        public int RoundsToWin = 2;
        public int Clock;
        public int CurrentRound;
        public int RoundWinner;
        public int RoundLoser => RoundWinner == 0 ? 1 : 0;
        public int Winner;
        public MatchState MatchState;

        public int[] VictoryCounter;

        public FadeScreenMode FadeMode;
        public int FadeScreenIntensity;
        public int FadeTime;
        public int FadeProgress;
        public bool TimeUp => Clock < 0;

        private SakugaActor[] _fighters;

        string sceneToReturn;
        string winnerMessage;

        bool canReturn;

        public void Initialize(SakugaActor[] fighters, MatchSettings settings)
        {
            _fighters = fighters;
            Winner = -1;
            CurrentRound = 0;
            ClockLimit = settings.TimeLimit;
            Clock = Mathf.Max(0, ClockLimit * GlobalVariables.TicksPerSecond);
            RoundsToWin = settings.RoundsToWin;
            SetMatchInitialState(settings); //<< This will be useful soon
            sceneToReturn = settings.SelectedModeSettings.ReturnToScene;
            VictoryCounter = new int[_fighters.Length];
            for (int i = 0; i < VictoryCounter.Length; ++i)
            {
                VictoryCounter[i] = 0;
            }
            FadeScreenIntensity = 100;
            FadeIn(40);
        }

        public void Render()
        {
            Cards.Render();                
        }

        private void SetMatchInitialState(MatchSettings settings)
        {
            if (settings.SelectedModeSettings.ShowMatchIntro)
            {
                _fighters[0].PlayIntro(_fighters[1].Data.Profile.ShortName);
                _fighters[1].PlayIntro(_fighters[0].Data.Profile.ShortName);
                MatchState = MatchState.INTRO;
            }
            else if (settings.SelectedModeSettings.ShowMatchCards)
            {
                MatchState = MatchState.CATCHPHRASE;
            }
            else
                MatchState = MatchState.ROUND_RUNNING;
        }

        public void CheckVictoryConditions()
        {
            RoundWinner = 0; // Player 1 by default

            //Do not compare the current healths directly to not mess up with characters
            //with different base health values.
            //Convert them into percentages instead... but a bit bigger for extra precision
            int p1HealthPercentage = _fighters[0].Parameters.Health.CurrentValue * 1000 / _fighters[0].Data.MaxHealth;
            int p2HealthPercentage = _fighters[1].Parameters.Health.CurrentValue * 1000 / _fighters[1].Data.MaxHealth;
            
            if (_fighters[0].IsKO() && _fighters[1].IsKO())
            {
                RoundWinner = -1; // Double K.O.
                GD.Print("Double K.O....");
            }
            else if (p2HealthPercentage == p1HealthPercentage)
            {
                RoundWinner = _fighters.Length; // Draw
                GD.Print("Draw...");
            }
            else if (p2HealthPercentage > p1HealthPercentage)
                RoundWinner = 1; // Player 2 wins

            if (RoundWinner > -1 && RoundWinner < _fighters.Length)
            {
                VictoryCounter[RoundWinner]++;
                GD.Print($"Player {RoundWinner + 1} win the round {CurrentRound + 1}!");
                if (VictoryCounter[RoundWinner] == RoundsToWin)
                    Winner = RoundWinner;
            }

            CurrentRound++;


            if (TimeUp)
            {
                Cards.PlayKnockoutAnimation(-1, 3);
            }
            else
            {
                if (RoundWinner == 1)
                    if (p2HealthPercentage == 1000)
                        Cards.PlayKnockoutAnimation(1, 1);
                    else
                        Cards.PlayKnockoutAnimation(1, 0);
                else if (RoundWinner == 0)
                    if (p1HealthPercentage == 1000)
                        Cards.PlayKnockoutAnimation(0, 1);
                    else
                        Cards.PlayKnockoutAnimation(0, 0);
                else if (RoundWinner == -1)
                    Cards.PlayKnockoutAnimation(-1, 2);
            }

            DelayTimer.Start(Cards.GetStateDelay());
            MatchState = MatchState.ROUND_END;
        }

        public void Tick()
        {
            FadeController();
            DelayTimer.Run();
            Cards.Run();

            foreach (SakugaActor fighter in _fighters)
            {
                fighter.StanceManager.CanRun = MatchState == MatchState.ROUND_RUNNING;
            }

            switch (MatchState)
            {
                case MatchState.INTRO:
                    if (AnyButtonPressed())
                    {
                        ResetRound();
                    }
                    if (_fighters[0].StateManager.CurrentState == 0 && _fighters[1].StateManager.CurrentState == 0)
                    {
                        Cards.PlayCatchPhraseAnimation();
                        DelayTimer.Start(Cards.GetStateDelay());
                        MatchState = MatchState.CATCHPHRASE;
                    }
                    break;
                case MatchState.CATCHPHRASE:
                    if (!DelayTimer.IsRunning())
                    {
                        Cards.PlayRoundStartAnimation(CurrentRound, false);
                        DelayTimer.Start(Cards.GetStateDelay());
                        GD.Print($"Round {CurrentRound + 1}...");
                        MatchState = MatchState.ROUND_START;
                    }
                    break;
                case MatchState.ROUND_START:
                    if (!DelayTimer.IsRunning())
                    {
                        GD.Print("START!");
                        MatchState = MatchState.ROUND_RUNNING;
                    }
                    break;
                case MatchState.ROUND_RUNNING:
                    MatchLoop();
                    break;
                case MatchState.ROUND_END:
                    if (!DelayTimer.IsRunning() && MatchEndCondition())
                    {
                        DelayTimer.Start(100);

                        if (Winner < 0)
                        {
                            SelectWinnerToAnimate();
                            MatchState = MatchState.ROUND_WINNER;
                        }
                        else
                        {
                            int Loser = Winner == 0 ? 1 : 0;
                            _fighters[Winner].PlayOutro(_fighters[Loser].Data.Profile.ShortName, out winnerMessage);
                            GD.Print($"Player {Winner + 1} win the match!");
                            MatchState = MatchState.MATCH_OUTRO;
                        }
                    }
                    break;
                case MatchState.ROUND_WINNER:
                    if (!DelayTimer.IsRunning())
                    {
                        Cards.PlayPlayerWinAnimation(RoundWinner);
                        DelayTimer.Start(Cards.GetStateDelay());
                        MatchState = MatchState.ROUND_INTERLUDE;
                    }
                    if (AnyButtonPressed())
                    {
                        DelayTimer.Stop();
                        MatchState = MatchState.ROUND_INTERLUDE;
                    }
                    break;
                case MatchState.ROUND_INTERLUDE:
                    if (!DelayTimer.IsRunning())
                    {
                        ResetRound();
                    }
                    break;
                case MatchState.NEXT_ROUND_TRANSITION:
                    GoToRound();
                    break;
                case MatchState.MATCH_OUTRO: 
                    if (AnyButtonPressed() || !DelayTimer.IsRunning())
                    {
                        DelayTimer.Stop();
                        // Open results menu
                        CallDeferred("ShowResultScreen");
                        MatchState = MatchState.RESULTS;
                    }
                    break;
                case MatchState.RESULTS:
                    // Return to menu for now
                    if (AnyButtonPressed())
                    {
                        LoadingScreenManager.Instance.LoadScene(sceneToReturn);
                    }
                    break;
            }
        }

        private void MatchLoop()
        {
            bool p1TimeStop = _fighters[0].SuperFlashing || _fighters[0].CinematicState;
            bool p2TimeStop = _fighters[1].SuperFlashing || _fighters[1].CinematicState;

            if (!p1TimeStop && !p2TimeStop)
            {
                if (ClockLimit >= 0) Clock--;

                if (TimeUp)
                {
                    GD.Print($"Time up!");
                    CheckVictoryConditions();
                    return;
                }
            }

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

        public void ResetRound()
        {
            FadeOut(20);
            MatchState = MatchState.NEXT_ROUND_TRANSITION;
        }

        private void GoToRound()
        {
            if (FadeMode == FadeScreenMode.NONE)
            {
                for (int i = 0; i < _fighters.Length; i++)
                {
                    _fighters[i].Reset();
                }
                
                ResetTimer();
                Cards.PlayRoundStartAnimation(CurrentRound, false);
                DelayTimer.Start(Cards.GetStateDelay());
                FadeIn(40);
                MatchState = MatchState.ROUND_START;
                GD.Print($"Round {CurrentRound + 1}...");
            }
        }

        public void ResetTimer()
        {
            Clock = Mathf.Max(0, ClockLimit * GlobalVariables.TicksPerSecond);
        }

        public void FadeIn(int time)
        {
            FadeTime = time;
            FadeProgress = FadeTime;
            FadeMode = FadeScreenMode.FADE_IN;
        }

        public void FadeOut(int time)
        {
            FadeTime = time;
            FadeProgress = 0;
            FadeMode = FadeScreenMode.FADE_OUT;
        }

        public void FadeController()
        {
            if (FadeMode == FadeScreenMode.NONE) return;

            switch (FadeMode)
            {
                case FadeScreenMode.FADE_IN:
                    FadeProgress--;
                    break;
                case FadeScreenMode.FADE_OUT:
                    FadeProgress++;
                    break;
            }
            if (FadeTime > 0) FadeScreenIntensity = GlobalFunctions.IntLerp(0, 100, FadeTime, FadeProgress);

            if ((FadeMode == FadeScreenMode.FADE_IN && FadeProgress <= 0) ||
                (FadeMode == FadeScreenMode.FADE_OUT && FadeProgress >= FadeTime + 20))
                FadeMode = FadeScreenMode.NONE;
        }

        private bool MatchEndCondition()
        {
            if (RoundWinner == -1) return true;
            if (RoundWinner == 2) return _fighters[0].StateManager.CurrentState == 0 && _fighters[1].StateManager.CurrentState == 0;

            return _fighters[RoundWinner].StateManager.CurrentState == 0 && ((_fighters[RoundLoser].IsKO() && _fighters[RoundLoser].Body.IsOnGround && 
                    _fighters[RoundLoser].StateManager.StateEnded()) || _fighters[RoundLoser].StateManager.CurrentState == 0);
        }

        private void SelectWinnerToAnimate()
        {
            switch (RoundWinner)
            {
                case -1: // Double K.O
                    break;
                case 0: // Player 1 win
                    _fighters[0].PlayVictoryPose(_fighters[1].Data.Profile.ShortName);
                    break;
                case 1: // Player 2 win
                    _fighters[1].PlayVictoryPose(_fighters[0].Data.Profile.ShortName);
                    break;
                case 2: // Draw
                    _fighters[0].PlayDefeatPose();
                    _fighters[1].PlayDefeatPose();
                    break;
            }
        }

        private void ShowResultScreen()
        {
            if (ResultsScreen == null) return;
            if (ResultsScreen.Visible) return;

            Control winP1 = ResultsScreen.GetNode<Control>("WinBannerP1");
            Control winP2 = ResultsScreen.GetNode<Control>("WinBannerP2");
            Label VictoryMessage = ResultsScreen.GetNode<Label>("VictoryMessage");

            if (Winner == 0) 
            {
                winP1.GetNode<Label>("NamePlayer1").Text = _fighters[Winner].Data.Profile.ShortName;
                winP2.Visible = false;
            }
            else if (Winner == 1)
            {
                winP2.GetNode<Label>("NamePlayer2").Text = _fighters[Winner].Data.Profile.ShortName;
                winP1.Visible = false;
            }

            VictoryMessage.Text = winnerMessage;
            ResultsScreen.Visible = true;
        }

        private bool AnyButtonPressed()
        {
            foreach(SakugaActor fighter in _fighters)
            {
                if (fighter.Inputs.WasPressed(fighter.Inputs.CurrentHistory, PlayerInputs.ANY_BUTTON))
                    return true;
            }
            return false;
        }
    }
}
