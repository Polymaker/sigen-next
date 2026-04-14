using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SiGen.ViewModels.Dialogs;

namespace SiGen.UI.Dialogs;

public partial class DialogHostWindow : Window
{
    public DialogHostWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public void SetDialogContent<TResult>(UserControl dialogControl, DialogViewModelBase<TResult> viewModel)
    {
        var contentPresenter = this.FindControl<ContentPresenter>("DialogContent")!;
        contentPresenter.Content = dialogControl;
        this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        //Width = this.DesiredSize.Width;
        //Height = this.DesiredSize.Height; 

        DataContext = viewModel;
        Title = viewModel.Title;

        // Handle window closing
        Closing += async (s, e) =>
        {
            if (!viewModel.CanClose())
            {
                e.Cancel = true;
                return;
            }

            await viewModel.OnClosingAsync();

            // If the dialog hasn't been completed yet, cancel it
            if (!IsDialogCompleted(viewModel))
            {
                viewModel.CancelDialog();
            }
        };

        // Handle Escape key
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape && viewModel.CanClose())
                viewModel.CancelDialog();
        };
    }

    private static bool IsDialogCompleted<TResult>(DialogViewModelBase<TResult> viewModel)
    {
        // Check if the completion source task is completed
        try
        {
            var task = viewModel.GetResultAsync();
            return task.IsCompleted;
        }
        catch
        {
            return false;
        }
    }
}