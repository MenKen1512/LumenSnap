using System.Windows.Automation;
using System.Windows.Controls.Primitives;

namespace LumenSnap;

internal sealed class ColorPalette : UniformGrid
{
    internal static readonly (string Name, string Value)[] Colors =
    [ ("Белый", "#FFFFFF"), ("Чёрный", "#141F2E"), ("Красный", "#FF635C"), ("Оранжевый", "#FF9F43"), ("Жёлтый", "#FFD166"),
      ("Зелёный", "#63D7A1"), ("Бирюзовый", "#43D9D1"), ("Голубой", "#65B7FF"), ("Синий", "#6E83FF"), ("Фиолетовый", "#BC8CFF") ];
    internal string SelectedColor { get; private set; }
    private readonly List<(string Color, Button Button)> swatches = [];
    internal ColorPalette(string current, Action<string> changed, int columns = 5)
    {
        SelectedColor = current; Columns = columns; HorizontalAlignment = HorizontalAlignment.Left;
        foreach (var (name, value) in Colors)
        {
            var dot = new System.Windows.Shapes.Ellipse { Width = 18, Height = 18, Fill = Rendering.Brush(value), Stroke = Theme.Muted, StrokeThickness = .5 };
            var button = Theme.Button("", name, () => { SelectedColor = value; Mark(); changed(value); }, 28);
            button.Content = dot; button.Height = 28; button.Padding = new Thickness(4); button.Margin = new Thickness(2);
            AutomationProperties.SetHelpText(button, "Выбрать цвет: " + name); swatches.Add((value, button)); Children.Add(button);
        }
        Mark();
    }
    private void Mark()
    {
        foreach (var (color, button) in swatches)
        {
            bool selected = string.Equals(color, SelectedColor, StringComparison.OrdinalIgnoreCase);
            button.Tag = selected ? "selected" : "swatch";
            AutomationProperties.SetItemStatus(button, selected ? "Выбран" : "");
        }
    }
}
