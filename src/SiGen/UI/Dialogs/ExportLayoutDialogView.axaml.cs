using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.ViewModels.Dialogs;
using System.ComponentModel;

namespace SiGen.UI.Dialogs;

public partial class ExportLayoutDialogView : UserControl
{
    public ExportLayoutDialogView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        foreach(var expander in new[] { FretsExpander, StringsExpander, FingerboardExpander, CenterLineExpander, MediansExpander })
        {
            expander.Expanded += Expander_Expanded;
        }
    }

    private void Expander_Expanded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Expander expander)
            return;

        expander.BringIntoView();
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is ExportLayoutDialogViewModel viewModel)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Check initial state and expand sections that are already enabled
            ExpandIfEnabled(viewModel.ExportFrets, FretsExpander);
            ExpandIfEnabled(viewModel.ExportStrings, StringsExpander);
            ExpandIfEnabled(viewModel.ExportFingerboard, FingerboardExpander);
            ExpandIfEnabled(viewModel.ExportCenterLine, CenterLineExpander);
            ExpandIfEnabled(viewModel.ExportMedians, MediansExpander);

            ConfigTabControl.SelectedIndex = viewModel.IsExportingToPdf ? 0 : 1;
        }

    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ExportLayoutDialogViewModel viewModel)
            return;

        // Expand the corresponding expander when its export flag is set to true
        switch (e.PropertyName)
        {
            case nameof(ExportLayoutDialogViewModel.ExportFrets):
                ExpandIfEnabled(viewModel.ExportFrets, FretsExpander);
                break;

            case nameof(ExportLayoutDialogViewModel.ExportStrings):
                ExpandIfEnabled(viewModel.ExportStrings, StringsExpander);
                break;

            case nameof(ExportLayoutDialogViewModel.ExportFingerboard):
                ExpandIfEnabled(viewModel.ExportFingerboard, FingerboardExpander);
                break;

            case nameof(ExportLayoutDialogViewModel.ExportCenterLine):
                ExpandIfEnabled(viewModel.ExportCenterLine, CenterLineExpander);
                break;

            case nameof(ExportLayoutDialogViewModel.ExportMedians):
                ExpandIfEnabled(viewModel.ExportMedians, MediansExpander);
                break;
        }
    }

    private static void ExpandIfEnabled(bool isEnabled, Expander expander)
    {
        if (isEnabled && !expander.IsExpanded)
            expander.IsExpanded = true;
    }
}