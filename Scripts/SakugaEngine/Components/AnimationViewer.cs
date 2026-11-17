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

        public void SetDepth(int LayerSorting)
        {
            // TODO
        }

        public void ViewAnimations(AnimationSettings anim, int Frame)
        {
            if (!GlobalFunctions.Assert(anim != null, "AnimationViewer: Null/Invalid AnimationData detected!")) return;
            if (!GlobalFunctions.Assert(anim.SourceAnimation != null && anim.SourceAnimation.Trim() != "", "AnimationViewer: AnimationData does not contain a valid animation string!")) return;
            
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

            if (anim.Inverse)
                targetFrame = anim.AnimationRange.Y - targetFrame;

            for (int a = 0; a < players.Length; a++)
            {
                string prfx = "";
                if (prefix != null && prefix.Length > a) prfx = prefix[a];
                string finalGroupName = prfx + anim.AnimationGroup;
                string finalAnimName = prfx + anim.SourceAnimation;
                string finalAnimReference = anim.AnimationGroup == null || anim.AnimationGroup.Trim() == "" ? finalAnimName : (finalGroupName + "/" + finalAnimName);
                players[a].Play(finalAnimReference);
                players[a].Seek((targetFrame * anim.Speed) / (float)GlobalVariables.TicksPerSecond);
            }

            if (OffsetNode != null)
                OffsetNode.Position = anim.Offset;
        }
    }
}
