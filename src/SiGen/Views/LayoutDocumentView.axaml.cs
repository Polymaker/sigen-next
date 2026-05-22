using Avalonia.Controls;
using Avalonia.Threading;
using SiGen.Settings;
using SiGen.UI.LayoutViewer;
using SiGen.ViewModels.EditorPanels;
using System;

namespace SiGen.Views;

public partial class LayoutDocumentView : UserControl
{
    public ViewModels.LayoutDocumentViewModel? ViewModel => DataContext as ViewModels.LayoutDocumentViewModel;
    private LayoutViewerColorScheme currentColorScheme = new LayoutViewerColorScheme();

    public LayoutDocumentView()
    {
        InitializeComponent();
        //InitializeViewerColorScheme();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var settingsService = App.GetService<SiGen.Services.ISettingsService>();
        currentColorScheme = settingsService.Settings.LayoutViewerColorScheme.ToColorScheme();
        settingsService.ViewerThemeChanged += SettingsService_ViewerThemeChanged;
        settingsService.UnitSystemChanged += SettingsService_UnitSystemChanged;
        Viewer?.ColorScheme = currentColorScheme;
        Viewer?.UnitMode = settingsService.Settings.PreferredUnits;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        //if (ViewModel != null && !initialized)
        //{
        //    currentColorScheme = ViewModel.SettingsService.Settings.LayoutViewerColorScheme.ToColorScheme();
        //    Viewer.ColorScheme = currentColorScheme;
        //    ViewModel.SettingsService.ViewerThemeChanged += SettingsService_ViewerThemeChanged;
        //    initialized = true;
        //}
        //Viewer.ResetZoomAndTranslation();
    }

    private void SettingsService_ViewerThemeChanged(object? sender, LayoutViewerColorSchemeSettings colorSchemeSettings)
    {
        if (Viewer == null) return;

        currentColorScheme = colorSchemeSettings.ToColorScheme();
        Dispatcher.UIThread.Post(() =>
        {
            Viewer.ColorScheme = currentColorScheme;
        });
    }

    private void SettingsService_UnitSystemChanged(object? sender, Measuring.UnitSystem unitSystem)
    {
        if (Viewer == null) return;

        Dispatcher.UIThread.Post(() =>
        {
            Viewer.UnitMode = unitSystem;
        });
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
            previousModel.ActiveVisibilityFilters = Viewer.ActiveVisibilityFilters;
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
            Viewer.ActiveVisibilityFilters = documentViewModel.ActiveVisibilityFilters;
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

    private void InitializeViewerColorScheme()
    {
        if (Viewer == null) return;

        var settingsService = (App.Current as SiGen.App)?.Services.GetService(typeof(SiGen.Services.ISettingsService)) as SiGen.Services.ISettingsService;
        Viewer.ColorScheme = settingsService!.Settings.LayoutViewerColorScheme.ToColorScheme();
    }
}