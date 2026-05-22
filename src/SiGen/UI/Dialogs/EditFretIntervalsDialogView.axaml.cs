using Avalonia.Controls;
using Avalonia.Interactivity;
using SiGen.UI.Controls;
using SiGen.ViewModels.Dialogs;

namespace SiGen.UI.Dialogs;

public partial class EditFretIntervalsDialogView : UserControl
{
    private Button? _addButton;
    private NumericTextBox? _newIntervalTextBox;

    public EditFretIntervalsDialogView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (_newIntervalTextBox == null)
        {
            _newIntervalTextBox = this.FindControl<NumericTextBox>("NewIntervalTextBox");
            _addButton = this.FindControl<Button>("AddButton");

            if (_newIntervalTextBox != null)
            {
                _newIntervalTextBox.TextChanged += OnNewIntervalTextChanged;
                UpdateAddButtonState();
            }
        }
    }

    private void OnNewIntervalTextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateAddButtonState();
    }

    private void UpdateAddButtonState()
    {
        if (_addButton == null || _newIntervalTextBox == null)
            return;

        var text = _newIntervalTextBox.Text;
        var canAdd = !string.IsNullOrWhiteSpace(text) && 
                     double.TryParse(text, out var value) && 
                     value >= 0;

        _addButton.IsEnabled = canAdd;
    }

    private void OnAddButtonClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is EditFretIntervalsDialogViewModel viewModel && _newIntervalTextBox != null)
        {
            if (_newIntervalTextBox.Value.HasValue)
                viewModel.AddInterval(_newIntervalTextBox.Value.Value);
            _newIntervalTextBox.Value = null;
        }
    }
}
