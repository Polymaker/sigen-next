using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SiGen.UI.Dialogs;

public partial class EditFretsDialogView : UserControl
{
    public EditFretsDialogView()
    {
        InitializeComponent();
        OverrideInfoText.IsVisible = false;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        OverrideInfoText.IsVisible = true;
        OverrideInfoText.Measure(Bounds.Size);
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window != null) 
        {
            window.Height += OverrideInfoText.DesiredSize.Height + 8; //stackpanel spacing
        }
    }
}