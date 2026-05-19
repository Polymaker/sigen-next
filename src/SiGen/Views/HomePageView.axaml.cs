using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Data.Presets;
using SiGen.Settings;
using SiGen.ViewModels;

namespace SiGen.Views;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
    }

    private HomePageViewModel? ViewModel => DataContext as HomePageViewModel;

    private void RecentFilesListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is RecentFileModel recentFile)
        {
            if (DataContext is HomePageViewModel vm && vm.OpenRecentFileCommand.CanExecute(recentFile))
            {
                vm.OpenRecentFileCommand.Execute(recentFile);
            }
            // Optionally clear selection so user can click again
            listBox.SelectedItem = null;
        }
    }

    private void TemplateButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Parent != null && button.Parent.DataContext is LayoutTemplate template)
        {
            ViewModel?.OpenTemplate(template);
        }
    }

}