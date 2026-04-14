using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.ViewModels.Dialogs;
using System;
using System.Threading.Tasks;

namespace SiGen.UI.Dialogs;

public partial class EditTuningDialogView : UserControl
{
    public EditTuningDialogViewModel? ViewModel => DataContext as EditTuningDialogViewModel;
    private EditTuningDialogViewModel? previousModel;

    public EditTuningDialogView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        UpdateTuningComboboxPlaceHolder();
        if (ViewModel != null)
        {
            var model = ViewModel;
            Task.Factory.StartNew(() =>
            {
                _ = model.EstimateUnitWeights();
            });
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (previousModel != null)
        {
            previousModel.TuningPresets.CollectionChanged -= TuningPresets_CollectionChanged;
            previousModel = null;
        }

        if (ViewModel != null)
        {
            ViewModel.TuningPresets.CollectionChanged += TuningPresets_CollectionChanged;
            if (IsLoaded)
                UpdateTuningComboboxPlaceHolder();
            previousModel = ViewModel;

        }
    }


    private void TuningPresets_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateTuningComboboxPlaceHolder();
    }

    private void UpdateTuningComboboxPlaceHolder()
    {
        if (ViewModel == null) return;

        TuningSelector.PlaceholderText = ViewModel.TuningPresets.Count == 0 ? 
            Lang.Resources.EditTuningDialog_NoMatchingTuningsMessage :
            Lang.Resources.EditTuningDialog_NoTuningSelectedMessage;
    }
}