using Godot;
using SakugaEngine.Global;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class FrameDataEvent : Resource
    {
        [Export] public FrameDataCondition[] Conditions;
        [Export] public FrameDataAction[] Actions;

        public void RunEvent(ref SakugaActor Actor)
        {
            if (!CheckConditions(ref Actor)) return;

            ExecuteActions(ref Actor);
        }

        private bool CheckConditions(ref SakugaActor Actor)
        {
            if (Conditions == null || Conditions.Length == 0) return true;

            foreach(FrameDataCondition condition in Conditions)
            {
                if (!GlobalFunctions.Assert(condition != null, "Null FrameDataCondition found"))
                    return false;
                
                if (!condition.Check(ref Actor))
                    return false;
            }

            return true;
        }

        private void ExecuteActions(ref SakugaActor Actor)
        {
            foreach(FrameDataAction action in Actions)
            {
                if (GlobalFunctions.Assert(action != null, "Null FrameDataAction found"))
                    action.Execute(ref Actor);
            }
        }
    }
}
