using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class PlayStateAction : FrameDataAction
    {
        [Export] private int StateIndex;
        [Export] private string StateName;
        [Export] private bool Reset = true;
        public override void Execute(ref SakugaActor Actor)
        {
            if (Actor.StateManager == null) return;

            int nextState = StateIndex;
            if (StateIndex < 0 && StateName != "") nextState = Actor.StateManager.GetStateIndex(StateName);

            Actor.StateManager.PlayState(nextState, Reset);
        }
    }
}
