using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FighterState : Resource
    {
        [Export] public string StateName;
        [Export] public MasterStance BaseStance;
        [Export] public StateType Type;
        [Export] public AnimationData AnimationData;
        [Export] public FrameDataEvent[] TransitionEvents;
        [Export] public FrameDataEvent[] OnEnterEvents;
        [Export] public FrameDataEvent[] OnTickEvents;
        [Export] public FrameDataEvent[] OnExitEvents;
        [Export] public FrameDataEvent[] OnHitConfirmEvents;
        [Export] public FrameDataEvent[] OnHitReactionEvents;
        [ExportSubgroup("AI Flags")]
        [Export] public AIFlags AIFlags;


        // FrameDataEvent checks
        public void RunTransitionEvents(ref SakugaActor actor)
        {
            foreach(FrameDataEvent transitionEvent in TransitionEvents)
            {
                if (GlobalFunctions.Assert(transitionEvent != null, $"Null TransitionEvent found on FighterState '{StateName}'"))
                    transitionEvent.RunEvent(ref actor);
            }
        }

        public void RunOnEnterEvents(ref SakugaActor actor)
        {
            //GD.Print($"{StateName}: Running OnEnterEvents");
            foreach(FrameDataEvent enterEvent in OnEnterEvents)
            {
                if (GlobalFunctions.Assert(enterEvent != null, $"Null OnEnterEvent found on FighterState '{StateName}'"))
                    enterEvent.RunEvent(ref actor);
            }
        }

        public void RunOnTickEvents(ref SakugaActor actor)
        {
            foreach(FrameDataEvent tickEvent in OnTickEvents)
            {
                if (GlobalFunctions.Assert(tickEvent != null, $"Null OnTickEvent found on FighterState '{StateName}'"))
                    tickEvent.RunEvent(ref actor);
            }
        }

        public void RunOnExitEvents(ref SakugaActor actor)
        {
            //GD.Print($"{StateName}: Running OnExitEvents");
            foreach(FrameDataEvent exitEvent in OnExitEvents)
            {
                if (GlobalFunctions.Assert(exitEvent != null, $"Null OnExitEvent found on FighterState '{StateName}'"))
                    exitEvent.RunEvent(ref actor);
            }
        }

        public void RunOnHitConfirmEvents(ref SakugaActor actor)
        {
            foreach(FrameDataEvent hitConfirmEvent in OnHitConfirmEvents)
            {
                if (GlobalFunctions.Assert(hitConfirmEvent != null, $"Null OnHitConfirmEvent found on FighterState '{StateName}'"))
                    hitConfirmEvent.RunEvent(ref actor);
            }
        }

        public void RunOnHitReactionEvents(ref SakugaActor actor)
        {
            foreach(FrameDataEvent hitReactionEvent in OnHitReactionEvents)
            {
                if (GlobalFunctions.Assert(hitReactionEvent != null, $"Null OnHitReactionEvent found on FighterState '{StateName}'"))
                    hitReactionEvent.RunEvent(ref actor);
            }
        }
    }
}