using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class PlayAnnouncerLineAction : FrameDataAction
    {
        [Export] public int Index;

        public override void Execute(ref SakugaActor Actor)
        {
            if (AudioManager.Instance == null) return;
            
            AudioManager.Instance.PlayAnnouncerClip(Index);
        }
    }
}
