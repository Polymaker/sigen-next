using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using SiGen.Layouts.Data;
using System;

namespace SiGen.UI.EditorPanels;

public partial class InstrumentInfoEditorPanel : UserControl
{
    public InstrumentInfoEditorPanel()
    {
        InitializeComponent();
        StringsEditor.StringCountChanged += StringsEditor_StringCountChanged;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        //if (DataContext is ViewModels.EditorPanels.InstrumentInfoPanelViewModel viewModel && StringsEditor != null)
        //{
        //    StringsEditor.IsLeftHanded = viewModel.LeftHanded;
        //}
    }

    private void LeftHandedButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.EditorPanels.InstrumentInfoPanelViewModel viewModel)
        { 
            viewModel.LeftHanded = true;
            //StringsEditor.IsLeftHanded = true;
        }
    }

    private void RightHandedButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.EditorPanels.InstrumentInfoPanelViewModel viewModel)
        {
            viewModel.LeftHanded = false;
            //StringsEditor.IsLeftHanded = false;
        }
    }

    private void StringsEditor_StringCountChanged(object? sender, Controls.StringCountChangedEventArgs e)
    {
        if (DataContext is ViewModels.EditorPanels.InstrumentInfoPanelViewModel viewModel)
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
}