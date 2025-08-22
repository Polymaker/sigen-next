using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Layouts.Builders;
using SiGen.Utilities;
using SiGen.ViewModels;

namespace SiGen.Views;

public partial class MobileMainView : UserControl
{
    public MobileMainView()
    {
        InitializeComponent();
        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var config = LayoutTemplates.CreateSingleScaleConfig();
        var result = LayoutBuilder.Build(config);
        SILayoutViewer.Layout = result.Layout;
        SILayoutViewer.ResetZoomAndTranslation();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        var config = LayoutTemplates.CreateBassGuitarMultiscaleLayout();
        var result = LayoutBuilder.Build(config);
        SILayoutViewer.Layout = result.Layout;
        SILayoutViewer.ResetZoomAndTranslation();
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        var config = LayoutTemplates.CreateMandolinLayout();
        var result = LayoutBuilder.Build(config);
        SILayoutViewer.Layout = result.Layout;
        SILayoutViewer.ResetZoomAndTranslation();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        //var services = (App.Current as App)?.Services;

        var config = LayoutTemplates.CreateSingleScaleConfig();
        var result = LayoutBuilder.Build(config);
        SILayoutViewer.Layout = result.Layout;

    }
}