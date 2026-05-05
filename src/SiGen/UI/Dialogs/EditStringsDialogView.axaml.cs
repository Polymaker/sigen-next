using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SiGen.ViewModels.Dialogs;
using System.Threading.Tasks;

namespace SiGen.UI.Dialogs;

public partial class EditStringsDialogView : UserControl
{
    public EditStringsDialogViewModel? ViewModel => DataContext as EditStringsDialogViewModel;

    public EditStringsDialogView()
    {
        InitializeComponent();
        
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        if (ViewModel != null)
        {
            var model = ViewModel;
            Task.Factory.StartNew(() =>
            {
                _ = model.LoadAvailableStringSets();
            });
        }
    }

}