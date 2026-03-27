using Godot;

namespace SakugaEngine.Resources
{
    [GlobalClass]
    public partial class CheckCustomVariableCondition : FrameDataCondition
    {
        [Export] private int ByIndex;
        [Export] private string ByName;
        [Export] private int Value;
        [Export] public Global.CustomVariableMode Mode;
        [Export] public Global.CompareMode CompareMode;

        public override bool Check(ref SakugaActor Actor)
        {
            if (Actor.Parameters == null) return false;
            CustomVariable variable = null;
            if (ByIndex >=0) variable = Actor.Parameters.GetVariable(ByIndex);
            else if (ByName != "") variable = Actor.Parameters.GetVariable(ByName);
            if (variable == null) return false;

            bool compareValue = variable.CompareValue(Value, CompareMode);
            bool compareMode = variable.CurrentMode == Mode;

            return compareValue && compareMode;
        }
    }
}
