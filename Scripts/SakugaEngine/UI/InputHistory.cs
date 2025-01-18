using Godot;
using System.Collections.Generic;

namespace SakugaEngine.UI
{
    public partial class InputHistory : VBoxContainer
    {
        [Export] private InputHistoryElement[] elements;

        private int ElementsAmount => elements.Length;

        public void SetHistoryList(InputManager manager)
        {
            elements[0].SetHistory(manager.InputHistory[manager.CurrentHistory]);

            int el = 0;

            for (int e = ElementsAmount - 1; e >= 1; e--)
            {
                el++;
                int element = el + manager.CurrentHistory;
                elements[e].Visible = manager.InputHistory[element % Global.InputHistorySize].duration != 0;

                elements[e].SetHistory(manager.InputHistory[element % Global.InputHistorySize]);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < ElementsAmount; i++)
            {
                elements[i].Visible = false;
            }
        }
    }
}
