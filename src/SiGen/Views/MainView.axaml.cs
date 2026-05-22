using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Utilities;
using SiGen.ViewModels;
using System;
using System.Diagnostics;

namespace SiGen.Views;

public partial class MainView : UserControl
{
    public ViewModels.MainViewModel? ViewModel => DataContext as ViewModels.MainViewModel;

    public MainView()
    {
        InitializeComponent();
        DocumentsTabControl.TabReordered += DocumentsTabControl_TabReordered;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (ViewModel != null)
        {
            ViewModel.OpenHomePage();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        //ViewModel?.OpenHomePage();
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            RebuildRecentFilesMenu();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.RecentFiles)) {
            RebuildRecentFilesMenu();
        }
    }

    private void RebuildRecentFilesMenu()
    {
        RecentFilesMenu.Items.Clear();
        if (ViewModel != null)
        {
            foreach (var recent in ViewModel.RecentFiles)
                RecentFilesMenu.Items.Add(recent);
        }
        RecentFilesMenu.IsEnabled = RecentFilesMenu.Items.Count > 0;
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