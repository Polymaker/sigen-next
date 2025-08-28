using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using SiGen.Data.Common;
using SiGen.Layouts.Data;
using SiGen.ViewModels.EditorPanels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace SiGen.UI.EditorPanels;

public partial class InstrumentInfoEditorPanel : UserControl
{
    protected InstrumentInfoPanelViewModel? ViewModel => DataContext as InstrumentInfoPanelViewModel;

    private List<InstrumentTypeItem> InstrumentTypeItems { get; }

    private bool isSelectingInstrumentType;

    public InstrumentInfoEditorPanel()
    {
        InitializeComponent();
        InstrumentTypeItems = Enum.GetValues(typeof(InstrumentType))
                           .Cast<InstrumentType>()
                           .Select(it => new InstrumentTypeItem(it))
                           .ToList();
        InstrumentTypesCombo.SelectionChanged += InstrumentTypesCombo_SelectionChanged;
        
        
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        InstrumentTypesCombo.ItemsSource = InstrumentTypeItems;

        if (ViewModel != null)
        {
            isSelectingInstrumentType = true;
            InstrumentTypesCombo.SelectedItem = InstrumentTypeItems.FirstOrDefault(i => i.InstrumentType == ViewModel.InstrumentType);
            isSelectingInstrumentType = false;
        }

        var comboGrid = InstrumentTypesCombo.GetVisualDescendants().OfType<Grid>().FirstOrDefault();
        if (comboGrid != null)
        {
            comboGrid.ColumnDefinitions[1].Width = new GridLength(20);
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (ViewModel != null && IsLoaded)
        {
            isSelectingInstrumentType = true;
            InstrumentTypesCombo.SelectedItem = InstrumentTypeItems.FirstOrDefault(i => i.InstrumentType == ViewModel.InstrumentType);
            isSelectingInstrumentType = false;
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        //var parentGrid = this.GetVisualAncestors().FirstOrDefault(x => x is Grid) as Grid;
        //if (parentGrid != null)
        //{
        //    int columnIndex = Grid.GetColumn(this);
        //    var columnDefinition = parentGrid.ColumnDefinitions[columnIndex];

        //    Trace.WriteLine($"Column.ActualWidth = {columnDefinition.ActualWidth}, LayoutNameBox.Width = {LayoutNameBox.Bounds.Width}, Diff = {columnDefinition.ActualWidth - LayoutNameBox.Bounds.Width}");
        //    if (LayoutNameBox.MinWidth > 0)
        //        Trace.WriteLine($"LayoutNameBox.MinWidth = {LayoutNameBox.MinWidth}");

        //    if (columnDefinition.ActualWidth > 0)
        //        LayoutNameBox.MaxWidth = columnDefinition.ActualWidth - 134;

        //}
    }

    private void InstrumentTypesCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null && !isSelectingInstrumentType)
        {
            var selectedItem = InstrumentTypesCombo.SelectedItem as InstrumentTypeItem;
            if (selectedItem != null)
            {
                ViewModel.InstrumentType = selectedItem.InstrumentType;
            }
        }

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

    
}

public class InstrumentTypeItem
{
    public InstrumentType InstrumentType { get; }
    public string ImagePath => "/Assets/Icons/InstrumentType_" + InstrumentType.ToString() + ".svg";
    public InstrumentTypeItem(InstrumentType instrumentType)
    {
        InstrumentType = instrumentType;
    }
}