using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SiGen.Layouts.Data;

namespace SiGen.UI.EditorPanels;

public partial class StringsFretsEditorPanel : UserControl
{
    public StringsFretsEditorPanel()
    {
        InitializeComponent();
        StringsEditor.StringCountChanged += StringsEditor_StringCountChanged;
    }

    private void StringsEditor_StringCountChanged(object? sender, Controls.StringCountChangedEventArgs e)
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
}