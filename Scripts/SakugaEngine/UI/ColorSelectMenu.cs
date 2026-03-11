using Godot;
using SakugaEngine.Resources;
using System;

namespace SakugaEngine.UI
{
    public partial class ColorSelectMenu : Control
    {
        [Export] private PackedScene menuElement;
        [Export] private ScrollContainer scroll;
        [Export] private Control contents;
        private ColorSelectionElement[] elements;

        public void Activate(FighterElement fighter)
        {
            Visible = true;
            elements = new ColorSelectionElement[fighter.ColorPalettes.Length];
            for (int i = 0; i < fighter.ColorPalettes.Length; i++)
            {
                ColorSelectionElement element = menuElement.Instantiate() as ColorSelectionElement;
                element.Set($"{i + 1}: {fighter.ColorPalettes[i].PaletteName}");
                contents.AddChild(element);
                elements[i] = element;
            }
            SelectElement(0);
            scroll.GetVScrollBar().Scale = Vector2.Zero;
        }

        public void Deactivate()
        {
            foreach (Control child in contents.GetChildren())
            {
                child.QueueFree();
            }
            elements = [];
            Visible = false;
        }

        public void SelectElement(int index)
        {
            foreach (ColorSelectionElement element in elements)
            {
                element.Deselect();
            }
            elements[index].Select();
            scroll.EnsureControlVisible(elements[index]);
        }
    }
}
