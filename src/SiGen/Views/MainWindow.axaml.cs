using Avalonia.Controls;
using SiGen.ViewModels;

namespace SiGen.Views;

public partial class MainWindow : Window
{
    private bool isConfirmedClosing;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (isConfirmedClosing)
            return;

        MainViewModel? model = (Content as MainView)?.DataContext as MainViewModel;
        if (model == null)
            return;

        e.Cancel = true;

        bool success = await model.TryCloseAllUnsavedDocuments();
        if (!success)
            return;

        isConfirmedClosing = true;
        Close();
    }
}