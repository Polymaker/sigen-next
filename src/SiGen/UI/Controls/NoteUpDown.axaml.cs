using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SiGen.Measuring;
using SiGen.Physics;
using System;
using System.ComponentModel;

namespace SiGen.UI.Controls;

public partial class NoteUpDown : UserControl
{
    public static readonly StyledProperty<NoteAndOctave?> ValueProperty =
            AvaloniaProperty.Register<MeasureTextBox, NoteAndOctave?>(nameof(Value)/*, coerce: CoerceValue*/);

    //[TypeConverter(typeof(MeasureTypeConverter))]
    public NoteAndOctave? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public NoteUpDown()
    {
        InitializeComponent();
        this.GetObservable(ValueProperty).Subscribe(val => UpdateTextAndButtons(val));
        NoteInput.LostFocus += NoteInput_LostFocus;
        NoteInput.KeyUp += NoteInput_KeyUp;
        NoteInput.PointerWheelChanged += NoteInput_PointerWheelChanged;
    }

    private void NoteInput_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyManualText();
        }
    }

    private void NoteInput_LostFocus(object? sender, RoutedEventArgs e)
    {
        ApplyManualText();
    }

    private void NoteInput_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
            ChangePitch(1);
        else if (e.Delta.Y < 0)
            ChangePitch(-1);
    }

    private void ApplyManualText()
    {
        if (NoteAndOctave.TryParse(NoteInput.Text ?? string.Empty, out var result))
        {
            Value = result;
        }
        else
        {
            // Reset text to current valid Tuning if parse fails
            UpdateTextAndButtons(Value);
        }
    }

    private void OnIncrement(object sender, RoutedEventArgs e) => ChangePitch(1);
    private void OnDecrement(object sender, RoutedEventArgs e) => ChangePitch(-1);

    private void UpdateTextAndButtons(NoteAndOctave? value)
    {
        if (value != null)
        {
            NoteInput.Text = value.Value.ToStringFormatted(); // Or however you format E4
            UpButton.IsEnabled = !(value.Value.Note == NoteName.B && value.Value.Octave >= 8);
            DownButton.IsEnabled = !(value.Value.Note == NoteName.C && value.Value.Octave <= 0);
        }
        else
        {
            NoteInput.Text = string.Empty;
            UpButton.IsEnabled = false;
            DownButton.IsEnabled = false;
        }
    }

    private void ChangePitch(int semitones)
    {
        if (Value == null) return;

        // 1. Convert current state to a linear number (like MIDI)
        int currentTotal = (Value.Value.Octave * 12) + (int)Value.Value.Note;
        int newTotal = currentTotal + semitones;

        // 2. Deconstruct back to Note and Octave
        var newNote = (NoteName)(newTotal % 12);
        if (newNote < 0) newNote += 12; // Handle negative wraps

        int newOctave = (int)Math.Min(Math.Max(Math.Floor(newTotal / 12.0), 0), 8);

        // 3. Update the property (preserving the centOffset)
        Value = Value.Value with { Note = newNote, Octave = newOctave };
    }

    // Keyboard Support (Up/Down Arrows)
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Up) { ChangePitch(1); e.Handled = true; }
        else if (e.Key == Key.Down) { ChangePitch(-1); e.Handled = true; }
        base.OnKeyDown(e);
    }
}