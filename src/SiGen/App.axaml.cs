using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Utilities;
using Microsoft.Extensions.DependencyInjection;
using SiGen.DependencyInjection;
using SiGen.Measuring;
using SiGen.Services;
using SiGen.ViewModels;
using SiGen.Views;
using System;
using System.Globalization;
using System.Linq;

namespace SiGen;

public partial class App : Application
{

    public IServiceProvider Services { get; set; } = default!;

    public App(IServiceProvider services)
    {
        Services = services;
    }

    public App()
    {
        var collection = new ServiceCollection();
        collection.AddSiGenServices();
        collection.AddSingleton<IDialogService, MockDialogService>();
        Services = collection.BuildServiceProvider();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        collection.AddSiGenServices();


        CultureInfo.CurrentUICulture = new CultureInfo("en-CA");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            //collection.AddSingleton<IFileDialogService, DummyFileDialogService>();
            var mainWindow = new MainWindow();
            collection.AddSingleton<IDialogService, DesktopDialogService>(sp => new DesktopDialogService(mainWindow, sp));
            collection.AddSingleton<LayoutDocumentModelFactory>();
            Services = collection.BuildServiceProvider();
            mainWindow.DataContext = Services.GetService<DesktopMainViewModel>();
            desktop.MainWindow = mainWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            collection.AddSingleton<IDialogService, MockDialogService>();
            collection.AddSingleton<LayoutDocumentModelFactory>();
            Services = collection.BuildServiceProvider();
            singleViewPlatform.MainView = new MobileMainView
            {
                //DataContext = Services.GetService<MainViewModel>() ?? new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
    // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}