using Godot;
using SakugaEngine.Global;
using SakugaEngine.Resources;

namespace SakugaEngine
{
    [GlobalClass] [Tool]
    public partial class AnimationViewer : Node
    {
        [Export] private AnimationPlayer[] players;
        [Export] private string[] prefix;
        [Export] public Node3D OffsetNode;

        private int targetFrame = 0;

        public void ViewAnimations(AnimationSettings anim, int Frame)
        {
            if (anim == null)
                return;
            
            if (anim == null) return;
            if (anim.SourceAnimation == "") return;

            int modTarget = (Frame - anim.AtFrame) % anim.FrameStepping;

            if (anim.LimitRange)
            {
                targetFrame = (Frame - anim.AtFrame) + anim.AnimationRange.X;
                targetFrame = Mathf.Clamp(targetFrame, anim.AnimationRange.X, anim.AnimationRange.Y);
                if (anim.AnimationRange.Y > anim.AnimationRange.X)
                    targetFrame -= modTarget;
            }
            else
            {
                targetFrame = Frame - anim.AtFrame;
                targetFrame -= modTarget;
            }

            for (int a = 0; a < players.Length; a++)
            {
                string prfx = "";
                if (prefix != null && prefix.Length > a) prfx = prefix[a];
                players[a].SpeedScale = anim.Speed;
                players[a].Play(prfx + anim.SourceAnimation);
                players[a].Seek(targetFrame / (float)GlobalVariables.TicksPerSecond, true);
            }

            if (OffsetNode != null)
                OffsetNode.Position = anim.Offset;
        }
    }
}
