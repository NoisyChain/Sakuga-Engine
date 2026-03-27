using Godot;
using SakugaEngine.Game;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class RoundRunningCondition : FrameDataCondition
    {
        [Export] private bool Not;

        public override bool Check(ref SakugaActor Actor)
        {
            if (GameManager.Instance == null) return false;

            if (Not) return GameManager.Instance.Monitor.MatchState != Global.MatchState.ROUND_WINNER;
            return GameManager.Instance.Monitor.MatchState == Global.MatchState.ROUND_WINNER;
        }
    }
}
