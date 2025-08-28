using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Utilities;
using SiGen.ViewModels;
using System.Diagnostics;

namespace SiGen.Views;

public partial class DesktopMainView : UserControl
{
    public ViewModels.DesktopMainViewModel? ViewModel => DataContext as ViewModels.DesktopMainViewModel;

    public DesktopMainView()
    {
        InitializeComponent();
        DocumentsTabControl.TabReordered += DocumentsTabControl_TabReordered;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ViewModel?.OpenHomePage();
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.RecentFiles))
            RecentFilesMenu.ItemsSource = ViewModel?.RecentFiles;
    }

    private void DocumentsTabControl_TabReordered(object? sender, UI.Controls.TabReorderedEventArgs e)
    {
        ViewModel?.ReorderDocuments(e.OldIndex, e.NewIndex);
    }

    private void RecentFileMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.DataContext is RecentFileMenuModel recent)
        {
            ViewModel?.OpenDocumentFile(recent.FilePath);
        }
    }

}