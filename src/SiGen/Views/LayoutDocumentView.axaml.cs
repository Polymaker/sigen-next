using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SiGen.ViewModels.EditorPanels;
using System;
using System.Diagnostics;
using System.Linq;

namespace SiGen.Views;

public partial class LayoutDocumentView : UserControl
{
    public LayoutDocumentView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        //Viewer.ResetZoomAndTranslation();
    }

    private ViewModels.LayoutDocumentViewModel? previousModel;

    protected override void OnDataContextBeginUpdate()
    {
        if (DataContext is ViewModels.LayoutDocumentViewModel documentViewModel)
        {
            documentViewModel.IsBindingPanels = true;
            InfoPanel.DataContext = documentViewModel.GetPanelViewModel<InstrumentInfoPanelViewModel>();
            ScaleLengthPanel.DataContext = documentViewModel.GetPanelViewModel<ScaleLengthPanelViewModel>();
            SpacingPanel.DataContext = documentViewModel.GetPanelViewModel<StringSpacingPanelViewModel>();
            FingerboardPanel.DataContext = documentViewModel.GetPanelViewModel<FingerboardPanelViewModel>();
            StringsFretsPanel.DataContext = documentViewModel.GetPanelViewModel<StringsFretsPanelViewModel>();
            documentViewModel.IsBindingPanels = false;
        }
        base.OnDataContextBeginUpdate();
        
    }

    protected override void OnDataContextEndUpdate()
    {
        if (previousModel != null)
        {
            previousModel.IsZoomToFit = Viewer.IsZoomToFit;
            previousModel.LayoutZoom = Viewer.Zoom;
            previousModel.LayoutTrans = Viewer.Translation;
            previousModel.LayoutOrientation = Viewer.Orientation;
            previousModel.ActiveSnapFilters = Viewer.ActiveSnapFilters;
            previousModel.IsMeasureToolEnabled = Viewer.IsMeasureToolActive;
            previousModel.LayoutChanged -= DocumentViewModel_LayoutChanged;
            previousModel = null;
        }

        if (DataContext is ViewModels.LayoutDocumentViewModel documentViewModel)
        {
            Viewer.IsAssigningLayout = true;
            Viewer.Layout = null;
            Viewer.Orientation = documentViewModel.LayoutOrientation;
            Viewer.Zoom = documentViewModel.LayoutZoom;
            Viewer.ActiveSnapFilters = documentViewModel.ActiveSnapFilters;
            Viewer.IsMeasureToolActive = documentViewModel.IsMeasureToolEnabled;
            Viewer.Layout = documentViewModel.Layout;
            
            Viewer.Translation = documentViewModel.LayoutTrans;
            Viewer.IsAssigningLayout = false;
            documentViewModel.LayoutChanged += DocumentViewModel_LayoutChanged;
            previousModel = documentViewModel;
            if (documentViewModel.IsZoomToFit && !Viewer.IsZoomToFit)
            {
                Viewer.ResetZoomAndTranslation();
            }

        }
        base.OnDataContextEndUpdate();
    }

    private void DocumentViewModel_LayoutChanged(object? sender, EventArgs e)
    {
        if (DataContext is ViewModels.LayoutDocumentViewModel documentViewModel)
            Viewer.Layout = documentViewModel.Layout;
    }
}