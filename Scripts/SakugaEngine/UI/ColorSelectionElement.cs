using Godot;
using System;

namespace SakugaEngine.UI
{
    public partial class ColorSelectionElement : ColorRect
    {
        [Export] private Label ColorName;
        [Export] private Color DefaultColor;
        [Export] private Color SelectedColor;

        public void Set(string colorName)
        {
            ColorName.Text = colorName;
            Color = DefaultColor;
        }

        public void Select()
        {
            Color = SelectedColor;
        }

        public void Deselect()
        {
            Color = DefaultColor;
        }
    }
}
