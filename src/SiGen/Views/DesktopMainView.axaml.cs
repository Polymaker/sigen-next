using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Utilities;

namespace SiGen.Views;

public partial class DesktopMainView : UserControl
{
    public DesktopMainView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is ViewModels.DesktopMainViewModel desktopMainViewModel)
        {
            desktopMainViewModel.OpenDocument(new ViewModels.DocumentViewModel("Test", null, LayoutTemplates.CreateBassGuitarMultiscaleLayout()));
        }
    }
}