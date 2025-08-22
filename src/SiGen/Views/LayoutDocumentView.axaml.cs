using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;

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

    private ViewModels.DocumentViewModel? previousModel;

    protected override void OnDataContextBeginUpdate()
    {
        if (DataContext is ViewModels.DocumentViewModel documentViewModel)
        {
            //Viewer.IsFirstLayoutAssignation = true;
            //Trace.WriteLine($"BeginUpdate Document: '{documentViewModel.Title}' Zoom: {documentViewModel.LayoutZoom}, Trans: {documentViewModel.LayoutTrans}");
        }
        base.OnDataContextBeginUpdate();
        
    }

    protected override void OnDataContextEndUpdate()
    {
        if (previousModel != null)
        {
            previousModel.LayoutZoom = Viewer.Zoom;
            previousModel.LayoutTrans = Viewer.Translation;
            previousModel.LayoutOrientation = Viewer.Orientation;
        }

        if (DataContext is ViewModels.DocumentViewModel documentViewModel)
        {
            Viewer.IsAssigningLayout = true;
            Viewer.Layout = null;
            Viewer.Orientation = documentViewModel.LayoutOrientation;
            Viewer.Zoom = documentViewModel.LayoutZoom;
            Viewer.Layout = documentViewModel.Layout;
            
            Viewer.Translation = documentViewModel.LayoutTrans;
            Viewer.IsAssigningLayout = false;
            previousModel = documentViewModel;
        }
        base.OnDataContextEndUpdate();
    }
}