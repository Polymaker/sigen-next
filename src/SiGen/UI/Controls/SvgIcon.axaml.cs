using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace SiGen.UI.Controls;

public class SvgIcon : TemplatedControl
{
    public static readonly StyledProperty<string?> IconCssProperty =
        AvaloniaProperty.Register<SvgIcon, string?>(
            nameof(IconCss),
            inherits: true);

    public static readonly StyledProperty<string?> SvgImageProperty =
        AvaloniaProperty.Register<SvgIcon, string?>(nameof(SvgImage));

    private string? IconCss
    {
        get => GetValue(IconCssProperty);
        set => SetValue(IconCssProperty, value);
    }

    public string? SvgImage
    {
        get => GetValue(SvgImageProperty);
        set => SetValue(SvgImageProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ForegroundProperty)
            UpdateIconCss();
    }

    private void UpdateIconCss()
    {
        if (Foreground != null && Foreground is ISolidColorBrush solid)
        {
            string hex = GetColorHex(solid.Color);
            IconCss = $"path {{fill: {hex}; }}";
        }
        else
            IconCss = null;
        //else
        //{
        //    string hex = GetColorHex(Colors.Red);
        //    IconCss = $"path {{fill: {hex}; }}";
        //}
    }

    private static string GetColorHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}