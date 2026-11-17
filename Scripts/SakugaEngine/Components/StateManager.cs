using Godot;
using SakugaEngine.Resources;
using SakugaEngine.Global;


namespace SakugaEngine
{
    [GlobalClass]
    public partial class StateManager : Node
    {
        private SakugaActor _owner;

        public int CurrentState;
        public int CurrentStateFrame;

        public int CurrentAnimationSettings;

        public void Initialize(SakugaActor owner)
        {
            _owner = owner;
            PlayState(_owner.IsActive ? 0 : -1);
            CurrentStateFrame = -1;
        }
        public void Reset()
        {
            PlayState(_owner.IsActive ? 0 : -1);
            CurrentStateFrame = -1;
        }
        public void PlayState(int state, bool reset = false)
        {
            if (!GlobalFunctions.Assert(state >= 0, "Negative state detected. Ignoring command.")) return;
            if (!GlobalFunctions.Assert(state < _owner.Data.States.Length, $"State at index {state} doesn't exist. State list is not long enough.")) return;
            if (!GlobalFunctions.Assert(GetState(state) != null, $"Null/Invalid state detected at index {state}!")) return;

            GetCurrentState().RunOnExitEvents(ref _owner);
            
            if (CurrentState != state)
            {
                CurrentState = state;
                CurrentStateFrame = 0;

            }
            else if (reset)
                CurrentStateFrame = 0;
            
            CurrentAnimationSettings = 0;

            if (_owner.Body != null)
            {
                _owner.Body.CurrentHitboxSettings = 0;
                _owner.Body.HitConfirmed = false;
                _owner.Body.UpdateColliders();
                _owner.Body.ResetHits();
            }
            
            _owner.CancelConditions = CancelCondition.WHIFF_CANCEL | CancelCondition.KARA_CANCEL;

            GetCurrentState().RunOnEnterEvents(ref _owner);
        }

        public void RunState()
        {
            CurrentStateFrame++;
            AdvanceAnimationData();
            if (CurrentStateFrame >= GlobalVariables.KaraCancelWindow) _owner.CancelConditions &= ~CancelCondition.KARA_CANCEL;
        }

        public FighterState GetState(int index) => _owner.Data.States[index];
        public FighterState GetState(string name) => _owner.Data.States[GetStateIndex(name)];
        public FighterState GetCurrentState() => _owner.Data.States[CurrentState];
        public StateType CurrentStateType() => GetCurrentState().Type;
        public bool StateEnded() => CurrentStateFrame >= GetCurrentState().AnimationData.Duration;
        public int GetStateIndex(string name)
        {
            for (int i = 0; i < _owner.Data.States.Length; i++)
            {
                if (_owner.Data.States[i].StateName == name) return i;
            }
            GD.PrintErr($"SAKUGA ENGINE: State '{name}' not found.");
            return -1;
        }

        public void AdvanceAnimationData()
        {
            CurrentAnimationSettings = -1;
            var animData = _owner.StateManager.GetCurrentState().AnimationData;

            if (!GlobalFunctions.Assert(animData != null, $"State '{GetCurrentState().StateName}' does not contain an AnimationData.")) return;
            if (animData.Animations == null || animData.Animations.Length <= 0) return;
            if (animData.Animations.Length == 1) { CurrentAnimationSettings = 0; return; }

            for (int i = 0; i < animData.Animations.Length; i++)
            {
                int nextFrame = (i == animData.Animations.Length - 1) ?
                                CurrentStateFrame :
                                animData.Animations[i + 1].AtFrame - 1;
                
                if (CurrentStateFrame >= animData.Animations[i].AtFrame && CurrentStateFrame <= nextFrame)
                {
                    CurrentAnimationSettings = i;
                    break;
                }   
            }
        }

        public AnimationSettings GetCurrentAnimationSettings()
        {
            if (CurrentAnimationSettings >= 0)
                return GetCurrentState().AnimationData.Animations[CurrentAnimationSettings];
            else return null;
        }
    }
}
