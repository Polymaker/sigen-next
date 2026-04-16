using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Lang;
using SiGen.Layouts.Data;
using SiGen.UI.Utils;
using SiGen.ViewModels.EditorPanels;
using System;

namespace SiGen.UI.EditorPanels;

public partial class StringsFretsEditorPanel : UserControl
{
    private StringsFretsPanelViewModel? ViewModel => DataContext as StringsFretsPanelViewModel;
    private StringsFretsPanelViewModel? _attachedViewModel;

    public StringsFretsEditorPanel()
    {
        InitializeComponent();
        StringsEditor.StringCountChanged += StringsEditor_StringCountChanged;

    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        RebuildPresetFlyouts();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_attachedViewModel != null)
        {
            _attachedViewModel.InstrumentTypeChanged -= ViewModel_InstrumentTypeChanged;
            _attachedViewModel = null;
        }

        if (DataContext is StringsFretsPanelViewModel viewModel)
        {
            viewModel.InstrumentTypeChanged += ViewModel_InstrumentTypeChanged;
            _attachedViewModel = viewModel;

            if (IsLoaded) RebuildPresetFlyouts();

        }
    }

    private void ViewModel_InstrumentTypeChanged(object? sender, System.EventArgs e)
    {
        RebuildPresetFlyouts();
    }

    private void StringsEditor_StringCountChanged(object? sender, Controls.AddRemoveStringEventArgs e)
    {
        if (DataContext is ViewModels.EditorPanels.StringsFretsPanelViewModel viewModel)
        {

            switch (e.ChangeType)
            {
                case Controls.StringCountChangeType.AddedTreble:
                    viewModel.AddString(FingerboardSide.Treble);
                    break;
                case Controls.StringCountChangeType.RemovedTreble:
                    viewModel.RemoveString(FingerboardSide.Treble);
                    break;
                case Controls.StringCountChangeType.AddedBass:
                    viewModel.AddString(FingerboardSide.Bass);
                    break;
                case Controls.StringCountChangeType.RemovedBass:
                    viewModel.RemoveString(FingerboardSide.Bass);
                    break;
            }
        }
    }

    private void RebuildPresetFlyouts()
    {
        FretCountField.Info = null;
        FretCountEditor.Margin = new Thickness(0);
        if (ViewModel != null)
        {
            var provider = ViewModel.LayoutDocumentContext.InstrumentValuesProvider;
            if (provider == null) return;

            var fretCountPresets = provider.GetCommonFretsCount();
            if (fretCountPresets.Count == 0) return;

            var menuBuilder = new MenuFlyoutBuilder()
                .AddHeader(Lang.Resources.StringsFretsEditorPanel_CommonFretsPresetHeader)
                .AddSubText(Lang.Resources.PresetFlyout_ClickPresetToApply);

            foreach (var preset in fretCountPresets)
            {
                menuBuilder.AddOption($"{preset} {Lang.Resources.FretsLabel}", () =>
                {
                    if (ViewModel != null)
                        ViewModel.NumberOfFrets = preset;
                }, Avalonia.Layout.HorizontalAlignment.Center);
            }

            FretCountField.Info = menuBuilder.Build();
            FretCountEditor.Margin = new Thickness(28, 0, 0, 0);
        }
    }
}